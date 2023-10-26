using System;
using System.Collections.Generic;
using System.Globalization;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace JadScugs
{
    public class PuppetGown
    {
        public PuppetGown(PlayerGraphics owner, string texture)
        {
            this.divs = 11;
            this.owner = owner;
            this.texture = texture;
            this.clothPoints = new Vector2[this.divs, this.divs, 3];
            this.visible = false;
            this.needsReset = true;
        }

        public void Update()
        {
            if (!this.visible || this.owner.player.room == null)
            {
                this.needsReset = true;
                return;
            }
            if (this.needsReset)
            {
                Debug.Log("GownReset");
                for (int i = 0; i < this.divs; i++)
                {
                    for (int j = 0; j < this.divs; j++)
                    {
                        this.clothPoints[i, j, 1] = this.owner.player.bodyChunks[1].pos;
                        this.clothPoints[i, j, 0] = this.owner.player.bodyChunks[1].pos;
                        this.clothPoints[i, j, 2] *= 0f;
                    }
                }
                this.needsReset = false;
            }
            Vector2 vector = Vector2.Lerp(this.owner.head.pos, this.owner.player.bodyChunks[1].pos, 0.75f);
            if (this.owner.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                vector += new Vector2(0f, 4f);
            }
            Vector2 a = default(Vector2);
            if (this.owner.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                vector += new Vector2(0f, Mathf.Sin((float)this.owner.player.animationFrame / 6f * 2f * 3.1415927f) * 2f);
                a = new Vector2(0f, -11f + Mathf.Sin((float)this.owner.player.animationFrame / 6f * 2f * 3.1415927f) * -2.5f);
            }
            Vector2 bodyPos = vector;
            Vector2 vector2 = Custom.DirVec(this.owner.player.bodyChunks[1].pos, this.owner.player.bodyChunks[0].pos + Custom.DirVec(default(Vector2), this.owner.player.bodyChunks[0].vel) * 5f) * 1.6f;
            Vector2 perp = Custom.PerpendicularVector(vector2);
            for (int k = 0; k < this.divs; k++)
            {
                for (int l = 0; l < this.divs; l++)
                {
                    Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)k);
                    float num = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)l);
                    this.clothPoints[k, l, 1] = this.clothPoints[k, l, 0];
                    this.clothPoints[k, l, 0] += this.clothPoints[k, l, 2];
                    this.clothPoints[k, l, 2] *= 0.999f;
                    this.clothPoints[k, l, 2].y -= 1.1f * this.owner.player.EffectiveRoomGravity;
                    Vector2 vector3 = this.IdealPosForPoint(k, l, bodyPos, vector2, perp) + a * (-1f * num);
                    Vector3 vector4 = Vector3.Slerp(-vector2, Custom.DirVec(vector, vector3), num) * (0.01f + 0.9f * num);
                    this.clothPoints[k, l, 2] += new Vector2(vector4.x, vector4.y);
                    float num2 = Vector2.Distance(this.clothPoints[k, l, 0], vector3);
                    float num3 = Mathf.Lerp(0f, 9f, num);
                    Vector2 a2 = Custom.DirVec(this.clothPoints[k, l, 0], vector3);
                    if (num2 > num3)
                    {
                        this.clothPoints[k, l, 0] -= (num3 - num2) * a2 * (1f - num / 1.4f);
                        this.clothPoints[k, l, 2] -= (num3 - num2) * a2 * (1f - num / 1.4f);
                    }
                    for (int m = 0; m < 4; m++)
                    {
                        IntVector2 intVector = new IntVector2(k, l) + Custom.fourDirections[m];
                        if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < this.divs && intVector.y < this.divs)
                        {
                            num2 = Vector2.Distance(this.clothPoints[k, l, 0], this.clothPoints[intVector.x, intVector.y, 0]);
                            a2 = Custom.DirVec(this.clothPoints[k, l, 0], this.clothPoints[intVector.x, intVector.y, 0]);
                            float num4 = Vector2.Distance(vector3, this.IdealPosForPoint(intVector.x, intVector.y, bodyPos, vector2, perp));
                            this.clothPoints[k, l, 2] -= (num4 - num2) * a2 * 0.05f;
                            this.clothPoints[intVector.x, intVector.y, 2] += (num4 - num2) * a2 * 0.05f;
                        }
                    }
                }
            }
        }

        private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
        {
            float num = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)x);
            float t = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)y);
            return bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(9f, 11f, t) + dir * Mathf.Lerp(8f, -9f, t) * (1f + Mathf.Sin(3.1415927f * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
        }

        public Color Color(float f)
        {
            return Custom.HSL2RGB(Mathf.Lerp(0.38f, 0.32f, Mathf.Pow(f, 2f)), Mathf.Lerp(0f, 0.1f, Mathf.Pow(f, 1.1f)), Mathf.Lerp(0.7f, 0.3f, Mathf.Pow(f, 6f)));
        }

        public void InitiateSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh(texture, this.divs - 1);
            for (int i = 0; i < this.divs; i++)
            {
                for (int j = 0; j < this.divs; j++)
                {
                    this.clothPoints[i, j, 0] = this.owner.player.firstChunk.pos;
                    this.clothPoints[i, j, 1] = this.owner.player.firstChunk.pos;
                    this.clothPoints[i, j, 2] = new Vector2(0f, 0f);
                }
            }
        }

        public void ApplyPalette(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < this.divs; i++)
            {
                for (int j = 0; j < this.divs; j++)
                {
                    (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j * this.divs + i] = this.Color((float)i / (float)(this.divs - 1));
                }
            }
        }

        public void DrawSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[sprite].isVisible = (this.visible && this.owner.player.room != null);
            if (!sLeaser.sprites[sprite].isVisible)
            {
                return;
            }
            for (int i = 0; i < this.divs; i++)
            {
                for (int j = 0; j < this.divs; j++)
                {
                    (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * this.divs + j, Vector2.Lerp(this.clothPoints[i, j, 1], this.clothPoints[i, j, 0], timeStacker) - camPos);
                }
            }
        }

        private PlayerGraphics owner;

        private string texture;

        public int gownIndex;

        private int divs;

        public Vector2[,,] clothPoints;

        public bool visible;

        public bool needsReset;
    }
}
