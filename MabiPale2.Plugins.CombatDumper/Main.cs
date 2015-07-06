using MabiPale2.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace MabiPale2.Plugins.CombatDumper
{
	public class CombatScene
	{
		public uint CombatID;
		public uint LinkedSceneID;
		public byte HitCount;
		public byte Type;
		public byte Flags;
		public uint BlockedByShieldPosX;
		public uint BlockedByShieldPosZ;
		public long ShieldCasterId;

		public List<Combatant> Combatants;
		public DateTime Time; 

		public CombatScene()
		{
			Combatants = new List<Combatant>();
		}
	}

	public struct Combatant
	{
		public uint CombatID;
		public long CharID;
		public byte Flags;
		public ushort Stun;
		public ushort SkillID;
		public ushort SecondarySkillID;
		// Attacker
		public long targetID;
		public uint AttackerFlags;
		public byte UsedWeaponSet;
		public byte WeaponParameterType;
		public uint AttackPosX;
		public uint AttackPosY;
		public uint DashedPosX;
		public uint DashedPosY;
		public uint DashDelay;
		public byte Phase;
		public long IndirectAttackerID;
		// Defender
		public uint DefenderFlags;
		public float Damage;
		public float Wound;
		public uint ManaDamage;
		public float DirectionX;
		public float DirectionY;
		public float DownPosX;
		public float DownPosY;
		public uint Unknown;
		public uint MultiHitDamageCount;
		public uint MultiHitDamageShowTime;
		public byte EffectFlags;
		public uint HitDelay;
		public long AttackerID;
	}

	public class Main : Plugin
	{
		private System.Windows.Forms.SaveFileDialog SaveCombatDumpDialog;
		private List<CombatScene> CombatActionsList;

		public override string Name
		{
			get { return "Combat Dumper"; }
		}

		public Main(IPluginManager pluginManager)
			: base(pluginManager)
		{

			this.CombatActionsList = new List<CombatScene>();
			this.SaveCombatDumpDialog = new SaveFileDialog
			{
				DefaultExt = "csv",
				Filter = "CSV Files|*.csv|All Files|*.*"
			};
		}

		public override void Initialize()
		{
			manager.AddToMenu(Name, OnClick);
			manager.Recv += OnRecv;
		}

		private void OnClick(object sender, EventArgs e)
		{
			SaveCombatDumpDialog.FileName = "combatDump_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

			if (SaveCombatDumpDialog.ShowDialog() == DialogResult.Cancel)
				return;

			try
			{
				using (var stream = SaveCombatDumpDialog.OpenFile())
				using (var sw = new StreamWriter(stream))
				{
					const string delim = " ; ";
					var sb = new StringBuilder();
					sb.AppendLine("Time ; CombatID ; LinkedSceneID ; HitCount ; Type ; Flags ; BlockedByShieldPosX ; BlockedByShieldPosZ ; ShieldCasterId ; CombatID ; CharID ; Flags ; Stun ; SkillID ; SecondarySkillID ; targetID ; AttackerFlags ; UsedWeaponSet ; WeaponParameterType ; AttackPosX ; AttackPosY ; DashedPosX ; DashedPosY ; DashDelay ; Phase ; IndirectAttackerID ; DefenderFlags ; Damage ; Wound ; ManaDamage ; DirectionX ; DirectionY ; DownPosX ; DownPosY ; Unknown ; MultiHitDamageCount ; MultiHitDamageShowTime ; EffectFlags ; HitDelay ; AttackerID");
					foreach (var scene in this.CombatActionsList)
						foreach (var combatant in scene.Combatants)
						{
							var sbLine = new StringBuilder();
							var time = scene.Time.ToString("hh:mm:ss.fff");
							sbLine.Append(time); sbLine.Append(delim);
							sbLine.Append(scene.CombatID); sbLine.Append(delim);
							sbLine.Append(scene.LinkedSceneID); sbLine.Append(delim);
							sbLine.Append(scene.HitCount); sbLine.Append(delim);
							sbLine.Append(scene.Type); sbLine.Append(delim);
							sbLine.Append(Convert.ToString(scene.Flags, 2).PadLeft(8, '0')); sbLine.Append(delim);
							sbLine.Append(scene.BlockedByShieldPosX); sbLine.Append(delim);
							sbLine.Append(scene.BlockedByShieldPosZ); sbLine.Append(delim);
							sbLine.Append(scene.ShieldCasterId.ToString("X16")); sbLine.Append(delim);

							// Combatant
							sbLine.Append(combatant.CombatID); sbLine.Append(delim);
							sbLine.Append(combatant.CharID.ToString("X16")); sbLine.Append(delim);
							sbLine.Append(Convert.ToString(combatant.Flags, 2).PadLeft(8, '0')); sbLine.Append(delim);
							sbLine.Append(combatant.Stun); sbLine.Append(delim);
							sbLine.Append(combatant.SkillID); sbLine.Append(delim);
							sbLine.Append(combatant.SecondarySkillID); sbLine.Append(delim);
							sbLine.Append(combatant.targetID.ToString("X16")); sbLine.Append(delim);
							sbLine.Append(Convert.ToString(combatant.AttackerFlags, 2).PadLeft(32, '0')); sbLine.Append(delim);
							sbLine.Append(combatant.UsedWeaponSet); sbLine.Append(delim);
							sbLine.Append(combatant.WeaponParameterType); sbLine.Append(delim);
							sbLine.Append(combatant.AttackPosX); sbLine.Append(delim);
							sbLine.Append(combatant.AttackPosY); sbLine.Append(delim);
							sbLine.Append(combatant.DashedPosX); sbLine.Append(delim);
							sbLine.Append(combatant.DashedPosY); sbLine.Append(delim);
							sbLine.Append(combatant.DashDelay); sbLine.Append(delim);
							sbLine.Append(combatant.Phase); sbLine.Append(delim);
							sbLine.Append(combatant.IndirectAttackerID.ToString("X16")); sbLine.Append(delim);
							sbLine.Append(Convert.ToString(combatant.DefenderFlags, 2).PadLeft(32, '0')); sbLine.Append(delim);
							sbLine.Append(combatant.Damage); sbLine.Append(delim);
							sbLine.Append(combatant.Wound); sbLine.Append(delim);
							sbLine.Append(combatant.ManaDamage); sbLine.Append(delim);
							sbLine.Append(combatant.DirectionX); sbLine.Append(delim);
							sbLine.Append(combatant.DirectionY); sbLine.Append(delim);
							sbLine.Append(combatant.DownPosX); sbLine.Append(delim);
							sbLine.Append(combatant.DownPosY); sbLine.Append(delim);
							sbLine.Append(combatant.Unknown); sbLine.Append(delim);
							sbLine.Append(combatant.MultiHitDamageCount); sbLine.Append(delim);
							sbLine.Append(combatant.MultiHitDamageShowTime); sbLine.Append(delim);
							sbLine.Append(Convert.ToString(combatant.EffectFlags, 2).PadLeft(8, '0')); sbLine.Append(delim);
							sbLine.Append(combatant.HitDelay); sbLine.Append(delim);
							sbLine.Append(combatant.AttackerID.ToString("X16")); sbLine.Append(delim);

							sb.AppendLine(sbLine.ToString());
						}
					sw.Write(sb.ToString());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to save file (" + ex.Message + ").", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnRecv(PalePacket palePacket)
		{
			if (palePacket.Op == 0x7926)
			{
				var scene = new CombatScene();
				scene.Time = palePacket.Time;
				scene.CombatID = palePacket.Packet.GetUInt();
				scene.LinkedSceneID = palePacket.Packet.GetUInt();
				scene.HitCount = palePacket.Packet.GetByte();
				scene.Type = palePacket.Packet.GetByte();
				scene.Flags = palePacket.Packet.GetByte();
				if ((scene.Flags & 1) == 1)
				{
					scene.BlockedByShieldPosX = palePacket.Packet.GetUInt();
					scene.BlockedByShieldPosZ = palePacket.Packet.GetUInt();
					scene.ShieldCasterId = palePacket.Packet.GetLong();
				}
				var count = palePacket.Packet.GetInt();
				for (var i = 0; i < count; ++i)
				{
					palePacket.Packet.Skip(1);
					var combatantBin = palePacket.Packet.GetBin();
					var combatantPacket = new Packet(combatantBin, 0);
					var combatant = new Combatant();
					combatant.CombatID = combatantPacket.GetUInt();
					combatant.CharID = combatantPacket.GetLong();
					combatant.Flags = combatantPacket.GetByte();
					combatant.Stun = combatantPacket.GetUShort();
					combatant.SkillID = combatantPacket.GetUShort();
					combatant.SecondarySkillID = combatantPacket.GetUShort();
					if ((combatant.Flags & 0x02) != 0)
					{
						combatant.targetID = combatantPacket.GetLong();
						combatant.AttackerFlags = combatantPacket.GetUInt();
						combatant.UsedWeaponSet = combatantPacket.GetByte();
						combatant.WeaponParameterType = combatantPacket.GetByte();
						combatant.AttackPosX = combatantPacket.GetUInt();
						combatant.AttackPosY = combatantPacket.GetUInt();
						if ((combatant.AttackerFlags & 0x10) != 0)
						{
							combatant.DashedPosX = combatantPacket.GetUInt();
							combatant.DashedPosY = combatantPacket.GetUInt();
							combatant.DashDelay = combatantPacket.GetUInt();
						}
						if ((combatant.AttackerFlags & 0x1000) != 0)
							combatant.Phase = combatantPacket.GetByte();

						if ((combatant.AttackerFlags & 0x08) != 0)
							combatant.IndirectAttackerID = combatantPacket.GetLong();
					}
					if ((combatant.Flags & 0x01) != 0)
					{
						combatant.DefenderFlags = combatantPacket.GetUInt();
						combatant.Damage = combatantPacket.GetFloat();
						combatant.Wound = combatantPacket.GetFloat();
						combatant.ManaDamage = combatantPacket.GetUInt();
						combatant.DirectionX = combatantPacket.GetFloat();
						combatant.DirectionY = combatantPacket.GetFloat();
						if ((combatant.DefenderFlags & 0x7CF00) != 0)
						{
							combatant.DownPosX = combatantPacket.GetFloat();
							combatant.DownPosY = combatantPacket.GetFloat();
							combatant.Unknown = combatantPacket.GetUInt();
						}
						if ((combatant.DefenderFlags & 0x2000000) != 0)
						{
							combatant.MultiHitDamageCount = combatantPacket.GetUInt();
							combatant.MultiHitDamageShowTime = combatantPacket.GetUInt();
						}
						combatant.EffectFlags = combatantPacket.GetByte();
						combatant.HitDelay = combatantPacket.GetUInt();
						combatant.AttackerID = combatantPacket.GetLong();
					}
					scene.Combatants.Add(combatant);
				}
				this.CombatActionsList.Add(scene);
			}
		}

	}
}
