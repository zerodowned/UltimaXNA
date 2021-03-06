﻿/***************************************************************************
 *   MapObjectCorpse.cs
 *   Based on code from ClintXNA's renderer: http://www.runuo.com/forums/xna/92023-hi.html
 *   
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
#region usings
using Microsoft.Xna.Framework;
using UltimaXNA.Entity;
using UltimaXNA.Rendering;
#endregion

namespace UltimaXNA.UltimaWorld.View
{
    public class MapObjectCorpse : MapObjectItem
    {
        public int BodyID { get; set; }
        public int FrameIndex { get; set; }

        public new int SortZ
        {
            get { return (int)Z; }
        }

        public int Layer
        {
            get { return SortTiebreaker; }
            set { SortTiebreaker = value; }
        }

        private bool m_noDraw = false;

        public MapObjectCorpse(Position3D position, int direction, BaseEntity ownerEntity, int nHue, int bodyID, float frame)
            : base(0x2006, position, direction, ownerEntity, nHue)
        {
            BodyID = bodyID;
            FrameIndex = (int)(frame * UltimaData.BodyConverter.DeathAnimationFrameCount(bodyID));

            UltimaData.AnimationFrame iFrame = getFrame();
            if (iFrame == null)
            {
                m_noDraw = true;
                return;
            }
            m_draw_texture = iFrame.Texture;
            m_draw_width = m_draw_texture.Width;
            m_draw_height = m_draw_texture.Height;
            m_draw_X = iFrame.Center.X - 22;
            m_draw_Y = iFrame.Center.Y + (int)(Z * 4) + m_draw_height - 22;
            m_draw_hue = Utility.GetHueVector(Hue);
            m_pickType = PickTypes.PickObjects;
            m_draw_flip = false;
        }

        internal override bool Draw(SpriteBatch3D sb, Vector3 drawPosition, MouseOverList molist, PickTypes pickType, int maxAlt)
        {
            if (m_noDraw)
                return false;
            return base.Draw(sb, drawPosition, molist, pickType, maxAlt);
        }

        private UltimaData.AnimationFrame getFrame()
        {
            UltimaData.AnimationFrame[] iFrames = UltimaData.AnimationData.GetAnimation(BodyID, UltimaData.BodyConverter.DeathAnimationIndex(BodyID), Facing, Hue);
            if (iFrames == null)
                return null;
            if (iFrames[FrameIndex].Texture == null)
                return null;
            return iFrames[FrameIndex];
        }

        public override string ToString()
        {
            return string.Format("MapObjectCorpse\n   BodyID:{1} Layer:{2}\n{0}", base.ToString(), BodyID, Layer);
        }
    }
}
