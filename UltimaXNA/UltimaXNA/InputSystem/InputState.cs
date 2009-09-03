﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UltimaXNA.Diagnostics;

namespace UltimaXNA.Input
{
    public enum MouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2
    }

    public class MacroKey
    {
        DateTime _lastExecution = DateTime.MinValue;
        bool _shift = false;
        bool _alt = false;
        bool _ctrl = false;
        Keys _key = Keys.A;

        public Keys Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public bool Shift
        {
            get { return _shift; }
            set { _shift = value; }
        }

        public bool Alt
        {
            get { return _alt; }
            set { _alt = value; }
        }

        public bool Ctrl
        {
            get { return _ctrl; }
            set { _ctrl = value; }
        }

        public DateTime LastExecution
        {
            get { return _lastExecution; }
            set { _lastExecution = value; }
        }

        public override bool Equals(object obj)
        {
            MacroKey key = obj as MacroKey;

            if(key == null)
                return false;

            return key.Key == Key && key.Shift == Shift && key.Alt == Alt && key.Ctrl == Ctrl;
        }

        public static bool operator ==(MacroKey l, MacroKey r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(MacroKey l, MacroKey r)
        {
            return !l.Equals(r);
        }

        public override int GetHashCode()
        {
            return (int)Key | ((Shift ? 1 : 0) << 9) | ((Alt ? 1 : 0) << 10) | ((Ctrl ? 1 : 0) << 11);
        }
    }

    public interface IMacroAction
    {
        string Name { get; }
        string Description { get; }

        void Execute();
    }

    public abstract class MacroAction : IMacroAction
    {
        static ILoggingService Log = new UltimaXNA.Diagnostics.Logger("MacroAction");

        public abstract string Name { get; }
        public abstract string Description { get; }

        public virtual void Execute()
        {
            Log.Debug("Action {0} was executed at {1}", Name, DateTime.Now);
        }
    }

    public class InputState : GameComponent, IInputService
    {
        public static TimeSpan KeyExecutionThreshold = TimeSpan.FromSeconds(0.1);

        private static Dictionary<MacroKey, IMacroAction> _macros = new Dictionary<MacroKey, IMacroAction>();

        private Dictionary<Keys, float> _keyCache;
        private Dictionary<MouseButtons, float> _mouseCache;

        public Dictionary<Keys, float> KeyCache
        {
            get { return _keyCache; }
            set { _keyCache = value; }
        }

        public Dictionary<MouseButtons, float> MouseCache
        {
            get { return _mouseCache; }
            set { _mouseCache = value; }
        }

        private KeyboardState _currentKeyState;        
        private KeyboardState _previousKeyState;

        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        private Vector2 _currentMousePosition;
        private Vector2 _previousMousePosition;

        public Vector2 CurrentMousePosition
        {
            get { return _currentMousePosition; }

        }

        public bool IsCursorMovedSinceLastUpdate()
        {
            if (_currentMousePosition == _previousMousePosition)
                return false;
            else 
                return true;
        }

        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseMove;

        public event EventHandler<KeyboardEventArgs> KeyDown;
        public event EventHandler<KeyboardEventArgs> KeyUp;

        public InputState(Game game)
            : base(game)
        {
            _keyCache = new Dictionary<Keys, float>();
            _previousKeyState = _currentKeyState = Keyboard.GetState();

            _mouseCache = new Dictionary<MouseButtons, float>();
            _previousMouseState = _currentMouseState = Mouse.GetState();

            _currentMousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                _keyCache.Add(key, 0.0f);
            }

            foreach (MouseButtons mb in Enum.GetValues(typeof(MouseButtons)))
            {
                _mouseCache.Add(mb, 0.0f);
            }
        }

        #region Mouse Input
        public bool IsMouseButtonPress(MouseButtons mb)
        {
            return (IsMouseButtonDown(mb) &&
                IsMouseButtonState(mb, ButtonState.Released,
                    ref _previousMouseState));
        }

        public bool IsMouseButtonRelease(MouseButtons mb)
        {
            return (IsMouseButtonUp(mb) &&
                IsMouseButtonState(mb, ButtonState.Pressed,
                    ref _previousMouseState));
        }

        public bool IsMouseButtonDown(MouseButtons mb)
        {
            return IsMouseButtonState(mb, ButtonState.Pressed,
                ref _currentMouseState);
        }

        public bool IsMouseButtonUp(MouseButtons mb)
        {
            return IsMouseButtonState(mb, ButtonState.Released,
                ref _currentMouseState);
        }

        private bool IsMouseButtonState(MouseButtons mb,
            ButtonState state, ref MouseState mouseState)
        {
            switch (mb)
            {
                case MouseButtons.LeftButton:
                    return (mouseState.LeftButton == state);
                case MouseButtons.MiddleButton:
                    return (mouseState.MiddleButton == state);
                case MouseButtons.RightButton:
                    return (mouseState.RightButton == state);
                case MouseButtons.XButton1:
                    return (mouseState.XButton1 == state);
                case MouseButtons.XButton2:
                    return (mouseState.XButton2 == state);
            }
            return false;
        }

        public float TimePressed(MouseButtons mb)
        {
            return _mouseCache[mb];
        }
        #endregion

        #region Keyboard Input
        public bool IsKeyPress(Keys key)
        {
            return (_currentKeyState.IsKeyDown(key) &&
                    _previousKeyState.IsKeyUp(key));
        }
                
        public bool IsKeyRelease(Keys key)
        {
            return (_currentKeyState.IsKeyUp(key) &&
                    _previousKeyState.IsKeyDown(key));
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _currentKeyState.IsKeyUp(key);
        }

        public float TimePressed(Keys key)
        {
            return _keyCache[key];
        }

        public bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
                (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                (keyNum >= (int)Keys.NumPad0 && keyNum <= (int)Keys.NumPad9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemQuotes || // '"
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus) // =+
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Key to char helper conversion method.
        /// Note: If the keys are mapped other than on a default QWERTY
        /// keyboard, this method will not work properly. Most keyboards
        /// will return the same for A-Z and 0-9, but the special keys
        /// might be different.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Char</returns>
        public char KeyToChar(Keys key, bool shiftPressed)
        {
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            }
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
                shiftPressed == false)
            {
                ret = (char)((int)'0' + (keyNum - Keys.D0));
            }
            else if (keyNum >= (int)Keys.NumPad0 && keyNum <= (int)Keys.NumPad9)
            {
                string name = Enum.GetName(typeof(Keys), key);
                ret = name.Substring(name.Length - 1)[0];
            }
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : '.';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : ',';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            return ret;
        }
        
        /// <summary>
        /// Handle keyboard input helper method to catch keyboard input
        /// for an input text. Only used to enter the player name in the game.
        /// </summary>
        /// <param name="inputText">Input text</param>
        public void HandleKeyboardInput(ref string inputText)
        {
            bool isShiftPressed = _currentKeyState.IsKeyDown(Keys.LeftShift) ||
                                  _currentKeyState.IsKeyDown(Keys.RightShift);

            foreach (Keys pressedKey in _currentKeyState.GetPressedKeys())
            {
                if (!_previousKeyState.GetPressedKeys().Contains(pressedKey))
                {
                    if (IsSpecialKey(pressedKey) == false && inputText.Length < 128)
                    {
                        inputText += KeyToChar(pressedKey, isShiftPressed);
                    }
                    else if (pressedKey == Keys.Back && inputText.Length > 0)
                    {
                        inputText = inputText.Substring(0, inputText.Length - 1);
                    }
                    else
                    {

                    }
                }
            }
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive)
                return;

            float elapsed = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            _previousKeyState = _currentKeyState;
            _previousMouseState = _currentMouseState;

            _currentMouseState = Mouse.GetState();
            _currentKeyState = Keyboard.GetState();

            _previousMousePosition.X = _currentMousePosition.X;
            _previousMousePosition.Y = _currentMousePosition.Y;

            _currentMousePosition.X = _currentMouseState.X;
            _currentMousePosition.Y = _currentMouseState.Y;

            #region Keybaord
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (IsKeyDown(key))
                {
                    _keyCache[key] += elapsed;
                }
                else if(IsKeyRelease(key))
                {

                }
                else
                {
                    _keyCache[key] = 0.0f;
                }
            }

            Keys[] keysDown = _currentKeyState.GetPressedKeys();

            Keys[] keysUp = (from k in (Keys[])Enum.GetValues(typeof(Keys))
                             where !keysDown.Contains(k) select k).ToArray();

            Keys[] keysPressed = (from k in _currentKeyState.GetPressedKeys()
                                  where IsKeyPress(k) select k).ToArray();

            Keys[] keysReleased = (from k in (Keys[])Enum.GetValues(typeof(Keys))
                                   where IsKeyRelease(k) select k).ToArray();

            KeyboardEventArgs keyboardEventArgs = new KeyboardEventArgs(keysUp, keysDown, keysPressed, keysReleased);

            if (keysReleased.Length > 0)
            {
                OnKeyUp(this, keyboardEventArgs);
            }

            if (keysPressed.Length > 0)
            {
                OnKeyDown(this, keyboardEventArgs);
            }
            #endregion

            #region Macros
            MacroKey[] macros = (from m in _macros.Keys
                                 where (IsKeyDown(m.Key) &&
                                        (m.Shift ? (IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift)) : true) &&
                                        (m.Alt ? (IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt)) : true) &&
                                        (m.Ctrl ? (IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl)) : true) &&
                                        (DateTime.Now > (m.LastExecution + KeyExecutionThreshold)))
                                 select m).ToArray();

            foreach (MacroKey macroKey in macros)
            {
                macroKey.LastExecution = DateTime.Now;
                _macros[macroKey].Execute();
            }
            #endregion

            #region Mouse
            foreach (MouseButtons mb in Enum.GetValues(typeof(MouseButtons)))
            {
                if (IsMouseButtonDown(mb))
                {
                    _mouseCache[mb] += elapsed;
                }
                else
                {
                    _mouseCache[mb] = 0.0f;
                }
            }

            MouseButtons[] buttonsDown = (from m in (MouseButtons[])Enum.GetValues(typeof(MouseButtons))
                                          where IsMouseButtonDown(m)
                                          select m).ToArray();
            MouseButtons[] buttonsUp = (from m in (MouseButtons[])Enum.GetValues(typeof(MouseButtons))
                                       where !buttonsDown.Contains(m) 
                                       select m).ToArray();
            MouseButtons[] buttonsPressed = (from m in (MouseButtons[])Enum.GetValues(typeof(MouseButtons))
                                             where IsMouseButtonPress(m)
                                             select m).ToArray();
            MouseButtons[] buttonsReleased = (from m in (MouseButtons[])Enum.GetValues(typeof(MouseButtons))
                                              where IsMouseButtonRelease(m)
                                              select m).ToArray();


            MouseEventArgs mouseEventArgs = new MouseEventArgs(buttonsDown, buttonsUp, buttonsPressed, 
                buttonsReleased, _currentMousePosition, _currentMousePosition - _previousMousePosition);

            if (buttonsReleased.Length > 0 && IsMouseCursorVisible())
            {
                OnMouseUp(this, mouseEventArgs);
            }

            if (buttonsPressed.Length > 0 && IsMouseCursorVisible())
            {
                OnMouseDown(this, mouseEventArgs);
            }

            if (_currentMousePosition != _previousMousePosition)
            {
                OnMouseMove(this, mouseEventArgs);
            }
            #endregion

            base.Update(gameTime);
        }

        public bool IsMouseCursorVisible()
        {
            PresentationParameters pp = Game.GraphicsDevice.PresentationParameters; 

            return (_currentMousePosition.X >= 0 && _currentMousePosition.X <= pp.BackBufferWidth &&
                   _currentMousePosition.Y >= 0 && _currentMousePosition.Y <= pp.BackBufferHeight);
        }

        private void OnKeyDown(object sender, KeyboardEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown(sender, e);
            }
        }

        private void OnKeyUp(object sender, KeyboardEventArgs e)
        {
            if (KeyUp != null)
            {
                KeyUp(sender, e);
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(sender, e);
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(sender, e);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMove != null)
            {
                MouseMove(sender, e);
            }
        }

        public static void RegisterMacro(MacroKey key, IMacroAction action)
        {
            if (action == null)
            {
                throw new NullReferenceException("macro");
            }

            if (_macros.ContainsKey(key))
            {
                _macros[key] = action;
            }
            else
            {
                _macros.Add(key, action);
            }
        }

        public static void ClearMacro(MacroKey key)
        {
            if (_macros.ContainsKey(key))
            {
                _macros.Remove(key);
            }
        }
    }
}
