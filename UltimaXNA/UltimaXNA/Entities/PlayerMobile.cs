﻿/***************************************************************************
 *   PlayerMobile.cs
 *   Part of UltimaXNA: http://code.google.com/p/ultimaxna
 *   
 *   begin                : May 31, 2009
 *   email                : poplicola@ultimaxna.com
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
#region usings
using System;
using Microsoft.Xna.Framework;
using UltimaXNA.TileEngine;
#endregion

namespace UltimaXNA.Entities
{
    public class PlayerMobile : Mobile
    {
        public PlayerMobile(Serial serial, World world)
            : base(serial, world)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " | " + Name;
        }

        public override void Update(GameTime gameTime)
        {
            animation.HoldAnimationMS = gameTime.ElapsedRealTime.Milliseconds * 2;
            base.Update(gameTime);
        }

        internal override void Draw(UltimaXNA.TileEngine.MapCell cell, Vector3 position, Vector3 positionOffset)
        {
            base.Draw(cell, position, positionOffset);
        }
    }
}
