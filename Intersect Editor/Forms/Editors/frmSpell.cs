﻿/*
    Intersect Game Engine (Editor)
    Copyright (C) 2015  JC Snider, Joe Bridges
    
    Website: http://ascensiongamedev.com
    Contact Email: admin@ascensiongamedev.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
using Intersect_Editor.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Intersect_Editor.Classes.Core;
using Intersect_Editor.Classes.General;

namespace Intersect_Editor.Forms
{
    public partial class frmSpell : Form
    {
        private ByteBuffer[] _spellsBackup;
        private bool[] _changed;
        private int _editorIndex;

        public frmSpell()
        {
            InitializeComponent();
        }

        private void frmSpell_Load(object sender, EventArgs e)
        {
            scrlProjectile.Maximum = Options.MaxProjectiles;
            scrlEvent.Maximum = Options.MaxCommonEvents;
            lstSpells.SelectedIndex = 0;
            cmbSprite.Items.Clear();
            cmbSprite.Items.Add("None");
            string[] spellNames = GameContentManager.GetTextureNames(GameContentManager.TextureType.Spell);
            for (int i = 0; i < spellNames.Length; i++)
            {
                cmbSprite.Items.Add(spellNames[i]);
            }
            cmbTransform.Items.Clear();
            cmbTransform.Items.Add("None");
            string[] spriteNames = GameContentManager.GetTextureNames(GameContentManager.TextureType.Entity);
            for (int i = 0; i < spriteNames.Length; i++)
            {
                cmbTransform.Items.Add(spriteNames[i]);
            }
            UpdateEditor();
        }

        public void InitEditor()
        {
            _spellsBackup = new ByteBuffer[Options.MaxSpells];
            _changed = new bool[Options.MaxSpells];
            for (var i = 0; i < Options.MaxSpells; i++)
            {
                _spellsBackup[i] = new ByteBuffer();
                _spellsBackup[i].WriteBytes(Globals.GameSpells[i].SpellData());
                lstSpells.Items.Add((i + 1) + ") " + Globals.GameSpells[i].Name);
                _changed[i] = false;
            }
        }

        private void UpdateEditor()
        {
            _editorIndex = lstSpells.SelectedIndex;

            txtName.Text = Globals.GameSpells[_editorIndex].Name;
            txtDesc.Text = Globals.GameSpells[_editorIndex].Desc;
            cmbType.SelectedIndex = Globals.GameSpells[_editorIndex].Type;
            
            scrlCastDuration.Value = Globals.GameSpells[_editorIndex].CastDuration;
            lblCastDuration.Text = "Cast Time (secs): " + ((double)scrlCastDuration.Value / 10);

            scrlCooldownDuration.Value = Globals.GameSpells[_editorIndex].CooldownDuration;
            lblCooldownDuration.Text = "Cooldown (secs): " + ((double)scrlCooldownDuration.Value / 10);

            scrlCastAnimation.Value = Globals.GameSpells[_editorIndex].CastAnimation;
            if (scrlCastAnimation.Value == -1)
            {
                lblCastAnimation.Text = "Cast Animation: None";
            }
            else
            {
                lblCastAnimation.Text = "Cast Animation: " + (scrlCastAnimation.Value + 1) + ".  " + Globals.GameAnimations[scrlCastAnimation.Value].Name;
            }
            

            scrlHitAnimation.Value = Globals.GameSpells[_editorIndex].HitAnimation;
            if (scrlHitAnimation.Value == -1)
            {
                lblHitAnimation.Text = "Hit Animation: None";
            }
            else
            {
                lblHitAnimation.Text = "Hit Animation: " + (scrlHitAnimation.Value + 1) + ".  " + Globals.GameAnimations[scrlHitAnimation.Value].Name;
            }
            
            scrlAttackReq.Value = Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Attack];
            lblAttackReq.Text = "Attack: " + scrlAttackReq.Value;
            scrlDefenseReq.Value = Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Defense];
            lblDefenseReq.Text = "Defense: " + scrlDefenseReq.Value;
            scrlAbilityPwrReq.Value = Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.AbilityPower];
            lblAbilityPwrReq.Text = "Ability Pwr: " + scrlAbilityPwrReq.Value;
            scrlMagicResistReq.Value = Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.MagicResist];
            lblMagicResistReq.Text = "Magic Resist: " + scrlMagicResistReq.Value;
            scrlSpeedReq.Value = Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Speed];
            lblSpeedReq.Text = "Speed: " + scrlSpeedReq.Value;
            scrlLevelReq.Value = Globals.GameSpells[_editorIndex].LevelReq;
            lblLevelReq.Text = "Level: " + scrlLevelReq.Value;

            cmbSprite.SelectedIndex = cmbSprite.FindString(Globals.GameSpells[_editorIndex].Pic);
            if (cmbSprite.SelectedIndex > 0) { picSpell.BackgroundImage = Bitmap.FromFile("resources/spells/" + cmbSprite.Text); }
            else { picSpell.BackgroundImage = null; }

            txtHPCost.Text = Globals.GameSpells[_editorIndex].VitalCost[(int)Enums.Vitals.Health].ToString();
            txtManaCost.Text = Globals.GameSpells[_editorIndex].VitalCost[(int)Enums.Vitals.Mana].ToString();

            UpdateSpellTypePanels();

            _changed[_editorIndex] = true;
        }

        private void UpdateSpellTypePanels()
        {
            grpTargetInfo.Hide();
            grpBuffDebuff.Hide();
            grpWarp.Hide();
            grpDash.Hide();
            grpEvent.Hide();
            cmbTargetType.Enabled = true;

            if (cmbType.SelectedIndex == cmbType.Items.IndexOf("Combat Spell"))
            {
                grpTargetInfo.Show();
                grpBuffDebuff.Show();
                cmbTargetType.SelectedIndex = Globals.GameSpells[_editorIndex].TargetType;
                UpdateTargetTypePanel();

                txtHPDiff.Text = Globals.GameSpells[_editorIndex].VitalDiff[(int)Enums.Vitals.Health].ToString();
                txtManaDiff.Text = Globals.GameSpells[_editorIndex].VitalDiff[(int)Enums.Vitals.Mana].ToString();
                txtAttackBuff.Text = Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Attack].ToString();
                txtDefenseBuff.Text = Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Defense].ToString();
                txtSpeedBuff.Text = Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Speed].ToString();
                txtAbilityPwrBuff.Text = Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.AbilityPower].ToString();
                txtMagicResistBuff.Text = Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.MagicResist].ToString();

                chkHOTDOT.Checked = Convert.ToBoolean(Globals.GameSpells[_editorIndex].Data1);
                scrlBuffDuration.Value = Globals.GameSpells[_editorIndex].Data2;
                lblBuffDuration.Text = "Duration: " + ((double)scrlBuffDuration.Value / 10) + "s";
                scrlTick.Value = Globals.GameSpells[_editorIndex].Data2;
                lblTick.Text = "Tick: " + ((double)scrlTick.Value / 10) + "s";
                cmbExtraEffect.SelectedIndex = Globals.GameSpells[_editorIndex].Data3;
                
            }
            else if (cmbType.SelectedIndex == cmbType.Items.IndexOf("Warp to Map"))
            {
                grpWarp.Show();
                txtWarpChunk.Text = Globals.GameSpells[_editorIndex].Data1.ToString();
                scrlWarpX.Value = Globals.GameSpells[_editorIndex].Data2;
                lblWarpX.Text = "X: " + scrlWarpX.Value;
                scrlWarpY.Value = Globals.GameSpells[_editorIndex].Data3;
                lblWarpY.Text = "Y: " + scrlWarpY.Value;
                scrlWarpDir.Value = Globals.GameSpells[_editorIndex].Data4;
                lblWarpDir.Text = "Dir: " + Globals.IntToDir(Globals.GameSpells[_editorIndex].Data4);
            }
            else if (cmbType.SelectedIndex == cmbType.Items.IndexOf("Warp to Target"))
            {
                grpTargetInfo.Show();
                cmbTargetType.SelectedText = "Single Target (includes self)";
                cmbTargetType.Enabled = false;
                UpdateTargetTypePanel();
            }
            else if (cmbType.SelectedIndex == cmbType.Items.IndexOf("Dash"))
            {
                grpDash.Show();
                scrlRange.Value = Globals.GameSpells[_editorIndex].CastRange;
                lblRange.Text = "Range: " + scrlRange.Value;
                chkIgnoreMapBlocks.Checked = Convert.ToBoolean(Globals.GameSpells[_editorIndex].Data1);
                chkIgnoreActiveResources.Checked = Convert.ToBoolean(Globals.GameSpells[_editorIndex].Data2);
                chkIgnoreInactiveResources.Checked = Convert.ToBoolean(Globals.GameSpells[_editorIndex].Data3);
                chkIgnoreZDimensionBlocks.Checked = Convert.ToBoolean(Globals.GameSpells[_editorIndex].Data4);
            }
            else if (cmbType.SelectedIndex == cmbType.Items.IndexOf("Event"))
            {
                grpEvent.Show();
                scrlEvent.Value = Globals.GameSpells[_editorIndex].Data1;
                lblEvent.Text = "Event: " + scrlEvent.Value + " " + Globals.CommonEvents[scrlEvent.Value].MyName;
            }
        }

        private void UpdateTargetTypePanel()
        {
            lblHitRadius.Hide();
            scrlHitRadius.Hide();
            lblCastRange.Hide();
            scrlCastRange.Hide();
            lblProjectile.Hide();
            scrlProjectile.Hide();
            if (cmbTargetType.SelectedIndex == cmbTargetType.Items.IndexOf("Single Target (includes self)") && cmbType.SelectedIndex == cmbType.Items.IndexOf("Combat Spell"))
            {
                lblHitRadius.Show();
                scrlHitRadius.Show();
                scrlHitRadius.Value = Globals.GameSpells[_editorIndex].HitRadius;
                lblHitRadius.Text = "Hit Radius: " + scrlHitRadius.Value;
            }
            if (cmbTargetType.SelectedIndex < cmbTargetType.Items.IndexOf("Self"))
            {
                lblCastRange.Show();
                scrlCastRange.Show();
                scrlCastRange.Value = Globals.GameSpells[_editorIndex].CastRange;
                lblCastRange.Text = "Cast Range: " + scrlCastRange.Value;
            }
            if (cmbTargetType.SelectedIndex == cmbTargetType.Items.IndexOf("Linear (projectile)"))
            {
                lblProjectile.Show();
                scrlProjectile.Show();
                scrlProjectile.Value = Globals.GameSpells[_editorIndex].Data4;
                if (scrlProjectile.Value == 0)
                {
                    lblProjectile.Text = "Projectile: 0 None";
                }
                else
                {
                    lblProjectile.Text = "Projectile: " + scrlProjectile.Value + " " + Globals.GameProjectiles[scrlProjectile.Value - 1].Name;
                }
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Name = txtName.Text;
            lstSpells.Items[_editorIndex] = (_editorIndex + 1) + ") " + txtName.Text;
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbType.SelectedIndex != Globals.GameSpells[_editorIndex].Type)
            {
                Globals.GameSpells[_editorIndex].Type = (byte)cmbType.SelectedIndex;
                Globals.GameSpells[_editorIndex].Data1 = 0;
                Globals.GameSpells[_editorIndex].Data2 = 0;
                Globals.GameSpells[_editorIndex].Data3 = 0;
                Globals.GameSpells[_editorIndex].Data4 = 0;
                UpdateSpellTypePanels();
            }
        }

        private void cmbSprite_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Pic = cmbSprite.Text;
            if (cmbSprite.SelectedIndex > 0) { picSpell.BackgroundImage = Bitmap.FromFile("resources/spells/" + cmbSprite.Text); }
            else { picSpell.BackgroundImage = null; }
        }

        private void scrlCastDuration_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].CastDuration = scrlCastDuration.Value;
            lblCastDuration.Text = "Cast Time (secs): " + ((double)scrlCastDuration.Value / 10);
        }

        private void scrlCooldownDuration_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].CooldownDuration = scrlCooldownDuration.Value;
            lblCooldownDuration.Text = "Cooldown (secs): " + ((double)scrlCooldownDuration.Value / 10);
        }

        private void scrlCastAnimation_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].CastAnimation = scrlCastAnimation.Value;
            if (scrlCastAnimation.Value == -1)
            {
                lblCastAnimation.Text = "Cast Animation: None";
            }
            else
            {
                lblCastAnimation.Text = "Cast Animation: " + (scrlCastAnimation.Value + 1) + ".  " + Globals.GameAnimations[scrlCastAnimation.Value].Name;
            }
        }

        private void scrlHitAnimation_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].HitAnimation = scrlHitAnimation.Value;
            if (scrlHitAnimation.Value == -1)
            {
                lblHitAnimation.Text = "Hit Animation: None";
            }
            else
            {
                lblHitAnimation.Text = "Hit Animation: " + (scrlHitAnimation.Value + 1) + ".  " + Globals.GameAnimations[scrlHitAnimation.Value].Name;
            }
        }

        private void scrlAttackReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Attack] = scrlAttackReq.Value;
            lblAttackReq.Text = "Attack: " + scrlAttackReq.Value;
        }

        private void scrlDefenseReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Defense] = scrlDefenseReq.Value;
            lblDefenseReq.Text = "Defense: " + scrlDefenseReq.Value;
        }

        private void scrlSpeedReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.Speed] = scrlSpeedReq.Value;
            lblSpeedReq.Text = "Speed: " + scrlSpeedReq.Value;
        }

        private void scrlAbilityPwrReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.AbilityPower] = scrlAbilityPwrReq.Value;
            lblAbilityPwrReq.Text = "Ability Pwr: " + scrlAbilityPwrReq.Value;
        }

        private void scrlMagicResistReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].StatReq[(int)Enums.Stats.MagicResist] = scrlMagicResistReq.Value;
            lblMagicResistReq.Text = "Magic Resist: " + scrlMagicResistReq.Value;
        }

        private void scrlLevelReq_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].LevelReq = scrlLevelReq.Value;
            lblLevelReq.Text = "Level: " + scrlLevelReq.Value;
        }

        private void cmbTargetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].TargetType = cmbTargetType.SelectedIndex;
            UpdateTargetTypePanel();
        }

        private void scrlCastRange_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].CastRange = scrlCastRange.Value;
            lblCastRange.Text = "Cast Range: " + scrlCastRange.Value;
        }

        private void scrlHitRadius_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].HitRadius = scrlHitRadius.Value;
            lblHitRadius.Text = "Hit Radius: " + scrlHitRadius.Value;
        }

        private void txtHPDiff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtHPDiff.Text, out x);
            Globals.GameSpells[_editorIndex].VitalDiff[(int)Enums.Vitals.Health] = x;
        }

        private void txtManaDiff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtManaDiff.Text, out x);
            Globals.GameSpells[_editorIndex].VitalDiff[(int)Enums.Vitals.Mana] = x;
        }

        private void txtAttackBuff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtAttackBuff.Text, out x);
            Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Attack] = x;
        }

        private void txtDefenseBuff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtDefenseBuff.Text, out x);
            Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Defense] = x;
        }

        private void txtSpeedBuff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtSpeedBuff.Text, out x);
            Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.Speed] = x;
        }

        private void txtAbilityPwrBuff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtAbilityPwrBuff.Text, out x);
            Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.AbilityPower] = x;
        }

        private void txtMagicResistBuff_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtMagicResistBuff.Text, out x);
            Globals.GameSpells[_editorIndex].StatDiff[(int)Enums.Stats.MagicResist] = x;
        }

        private void chkHOTDOT_CheckedChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data1 = Convert.ToInt32(chkHOTDOT.Checked);
        }

        private void scrlBuffDuration_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data2 = scrlBuffDuration.Value;
            lblBuffDuration.Text = "Duration: " + ((double)scrlBuffDuration.Value / 10) + "s";
        }

        private void txtWarpChunk_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtWarpChunk.Text, out x);
            Globals.GameSpells[_editorIndex].Data1 = x;
        }

        private void scrlWarpX_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data2 = scrlWarpX.Value;
            lblWarpX.Text = "X: " + scrlWarpX.Value;
        }

        private void scrlWarpY_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data3 = scrlWarpY.Value;
            lblWarpY.Text = "Y: " + scrlWarpY.Value;
        }

        private void scrlWarpDir_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data4 = scrlWarpDir.Value;
            lblWarpDir.Text = "Dir: " + Globals.IntToDir(Globals.GameSpells[_editorIndex].Data4);
        }

        private void txtHPCost_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtHPCost.Text, out x);
            Globals.GameSpells[_editorIndex].VitalCost[(int)Enums.Vitals.Health] = x;
        }

        private void txtManaCost_TextChanged(object sender, EventArgs e)
        {
            int x = 0;
            int.TryParse(txtManaCost.Text, out x);
            Globals.GameSpells[_editorIndex].VitalCost[(int)Enums.Vitals.Mana] = x;
        }

        void lstSpells_Click(object sender, System.EventArgs e)
        {
            UpdateEditor();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var temp = new SpellStruct();
            Globals.GameSpells[_editorIndex].Load(temp.SpellData(),_editorIndex);
            UpdateEditor();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < Options.MaxSpells; i++)
            {
                Globals.GameSpells[i].Load(_spellsBackup[i].ToArray(),i);
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < Options.MaxItems; i++)
            {
                if (_changed[i])
                {
                    PacketSender.SendSpell(i, Globals.GameSpells[i].SpellData());
                }
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Desc = txtDesc.Text;
        }

        private void cmbExtraEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data3 = cmbExtraEffect.SelectedIndex;

            lblHint.Visible = true;
            lblSprite.Visible = false;
            cmbTransform.Visible = false;
            picSprite.Visible = false;

            if (cmbExtraEffect.SelectedIndex == cmbExtraEffect.Items.IndexOf("Transform"))
            {
                lblHint.Visible = false;
                lblSprite.Visible = true;
                cmbTransform.Visible = true;
                picSprite.Visible = true;

                cmbTransform.SelectedIndex = cmbTransform.FindString(Globals.GameSpells[_editorIndex].Data5);
                if (cmbTransform.SelectedIndex > 0) { picSpell.BackgroundImage = Bitmap.FromFile("resources/spells/" + cmbTransform.Text); }
                else { picSpell.BackgroundImage = null; }
            }
        }

        private void frmSpell_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.CurrentEditor = -1;
        }

        private void scrlRange_Scroll(object sender, ScrollEventArgs e)
        {
            lblRange.Text = "Range: " + scrlRange.Value;
            Globals.GameSpells[_editorIndex].CastRange = scrlRange.Value;
        }

        private void scrlProjectile_Scroll(object sender, ScrollEventArgs e)
        {
            if (scrlProjectile.Value == 0)
            {
                lblProjectile.Text = "Projectile: 0 None";
            }
            else
            {
                lblProjectile.Text = "Projectile: " + scrlProjectile.Value + " " + Globals.GameProjectiles[scrlProjectile.Value - 1].Name;
            }
            Globals.GameSpells[_editorIndex].Data4 = scrlProjectile.Value;
        }

        private void scrlTick_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data4 = scrlTick.Value;
            lblTick.Text = "Tick: " + ((double)scrlTick.Value / 10) + "s";
        }

        private void chkIgnoreMapBlocks_CheckedChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data1 = Convert.ToInt32(chkIgnoreMapBlocks.Checked);
        }

        private void chkIgnoreActiveResources_CheckedChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data2 = Convert.ToInt32(chkIgnoreActiveResources.Checked);
        }

        private void chkIgnoreInactiveResources_CheckedChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data3 = Convert.ToInt32(chkIgnoreInactiveResources.Checked);
        }

        private void chkIgnoreZDimensionBlocks_CheckedChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data4 = Convert.ToInt32(chkIgnoreZDimensionBlocks.Checked);
        }

        private void cmbTransform_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data5 = cmbTransform.Text;
            if (cmbTransform.SelectedIndex > 0) { picSprite.BackgroundImage = Bitmap.FromFile("resources/spells/" + cmbTransform.Text); }
            else { picSprite.BackgroundImage = null; }
        }

        private void scrlEvent_Scroll(object sender, ScrollEventArgs e)
        {
            Globals.GameSpells[_editorIndex].Data1 = scrlEvent.Value;
            lblEvent.Text = "Event: " + scrlEvent.Value + " " + Globals.CommonEvents[scrlEvent.Value].MyName;
        }

    }
}
