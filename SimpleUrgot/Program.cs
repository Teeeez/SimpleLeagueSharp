using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;

namespace SimpleUrgot
{
    class Program
    {

        public static string ChampName = "Urgot";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Obj_AI_Hero PlayerH = ObjectManager.Player;
        public static Spell Q, Q2, W, E, R;
        public static Items.Item DfgItem = new Items.Item(3128, 750);
        public static Items.Item BilgeItem = new Items.Item(3144, 450);
        public static Items.Item BladeItem = new Items.Item(3153, 450);
        public static Items.Item GhostItem = new Items.Item(3142, float.MaxValue);
        public static Items.Item MuraItem = new Items.Item(3042, float.MaxValue);

        public static Menu QbMenu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1000);
            Q.SetSkillshot(0.10f, 100f, 1600f, true, SkillshotType.SkillshotLine);
            Q2 = new Spell(SpellSlot.Q, 1200);
            Q2.SetSkillshot(0.10f, 100f, 1600f, false, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            E.SetSkillshot(0.283f, 50f, 1750f, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R, 550);

            //Base menu
            QbMenu = new Menu("Simple" + ChampName, ChampName, true);
            //Orbwalker and menu
            QbMenu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(QbMenu.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            QbMenu.AddSubMenu(ts);
            //Combo menu
            QbMenu.AddSubMenu(new Menu("Combo", "Combo"));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W").SetValue(false));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("useSW", "Smart W").SetValue(false));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E").SetValue(false));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("ComboMana", "Mana").SetValue(new Slider(1, 1, 100)));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Harass menu
            QbMenu.AddSubMenu(new Menu("Harass", "Harass"));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("useHQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("useHW", "Smart W").SetValue(false));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("useHE", "Use E").SetValue(false));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "Mana").SetValue(new Slider(40, 1, 100)));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("HarassKey", "Hahrass Key").SetValue(new KeyBind(67, KeyBindType.Press)));
            QbMenu.SubMenu("Harass").AddItem(new MenuItem("HarassKey2", "Harass Key 2").SetValue(new KeyBind(86, KeyBindType.Press)));
            //LaneClear
            QbMenu.AddSubMenu(new Menu("Laneclear", "Laneclear"));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("useLQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("useLE", "Use E").SetValue(false));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("LaneMana", "Mana").SetValue(new Slider(30, 1, 100)));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("LaneclearKey", "Laneclear Key").SetValue(new KeyBind(86, KeyBindType.Press)));
            //Lasthit
            QbMenu.AddSubMenu(new Menu("Lasthit", "Lasthit"));
            QbMenu.SubMenu("Lasthit").AddItem(new MenuItem("useLhQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Lasthit").AddItem(new MenuItem("LastMana", "Mana").SetValue(new Slider(40, 1, 100)));
            QbMenu.SubMenu("Lasthit").AddItem(new MenuItem("LasthitKey", "Lasthit Key").SetValue(new KeyBind(88, KeyBindType.Press)));
            //Ultimate & KS
            QbMenu.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("BotrkSteal", "Botrk").SetValue(false));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("BilgeSteal", "Bilgewater C").SetValue(false));
            //Items
            QbMenu.AddSubMenu(new Menu("Items", "Items"));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("bilge", "Bilgewater C").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("botrk", "Blade of the Ruined King").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("bomh", "Wait for max heal with blade").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("ghostbl", "Ghostblade").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("mura", "Muramana").SetValue(false));
            //Misc #soon
            QbMenu.AddSubMenu(new Menu("Misc", "Misc"));
            QbMenu.SubMenu("Misc").AddItem(new MenuItem("drawq", "Draw Q").SetValue(false));
            QbMenu.SubMenu("Misc").AddItem(new MenuItem("drawq2", "Draw Q2").SetValue(false));
            QbMenu.SubMenu("Misc").AddItem(new MenuItem("drawe", "Draw E").SetValue(false));

            QbMenu.AddItem(new MenuItem("madeby", "Made by Pataxx").DontSave());

            QbMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate; // onTick in bol

            Game.PrintChat("Simple" + ChampName + " loaded!");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (QbMenu.Item("ComboKey").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (QbMenu.Item("HarassKey").GetValue<KeyBind>().Active || QbMenu.Item("HarassKey2").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (QbMenu.Item("LaneclearKey").GetValue<KeyBind>().Active)
            {
                Laneclear();
            }
            if (QbMenu.Item("LasthitKey").GetValue<KeyBind>().Active)
            {
                Lasthit();
            }
            if (QbMenu.Item("BotrkSteal").GetValue<bool>() && BladeItem.IsReady())
            {
                BotrkSteal();
            }
            if (QbMenu.Item("BilgeSteal").GetValue<bool>() && BilgeItem.IsReady())
            {
                BilgeSteal();
            }
            if (!QbMenu.Item("mura").GetValue<bool>() && Player.HasBuff("Muramana") && MuraItem.IsReady())
            {
                MuraItem.Cast();
            }

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!QbMenu.Item("drawq").GetValue<bool>() && Q.IsReady())
            {
                Utility.DrawCircle(Player.ServerPosition,Q.Range,Color.Crimson);
            }

            if (!QbMenu.Item("drawq2").GetValue<bool>() && Q.IsReady())
            {
                Utility.DrawCircle(Player.ServerPosition, Q2.Range, Color.DarkRed);
            }

            if (!QbMenu.Item("drawE").GetValue<bool>() && E.IsReady())
            {
                Utility.DrawCircle(Player.ServerPosition, E.Range, Color.Chartreuse);
            }
        }

        public static void BilgeSteal()
        {
            var unit = ObjectManager.Get<Obj_AI_Hero>().First(obj => obj.IsValidTarget(600) && PlayerH.GetItemDamage(obj, Damage.DamageItems.Bilgewater) >= obj.Health);
            if (unit != null)
            {
                BilgeItem.Cast(unit);
            }
        }

        public static void BotrkSteal()
        {
            var unit = ObjectManager.Get<Obj_AI_Hero>().First(obj => obj.IsValidTarget(600) && PlayerH.GetItemDamage(obj, Damage.DamageItems.Botrk) >= obj.Health);
            if (unit != null)
            {
                BladeItem.Cast(unit);
            }
        }

        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Physical);
            if (target == null || Player.Mana <= Player.MaxMana * (QbMenu.Item("ComboMana").GetValue<int>() / 100.0)) return;

            var castQ = QbMenu.Item("useQ").GetValue<bool>();
            var castE = QbMenu.Item("useE").GetValue<bool>();
            var castW = QbMenu.Item("useW").GetValue<bool>();
            var castSw = QbMenu.Item("useSW").GetValue<bool>();

            if (QbMenu.Item("ghostbl").GetValue<bool>() && GhostItem.IsReady() && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                GhostItem.Cast();
            }
            if (QbMenu.Item("bilge").GetValue<bool>() && BilgeItem.IsReady() && target.IsValidTarget(BilgeItem.Range))
            {
                BilgeItem.Cast();
            }
            if (QbMenu.Item("botrk").GetValue<bool>() && BladeItem.IsReady() && target.IsValidTarget(BladeItem.Range))
            {
                if (QbMenu.Item("bomh").GetValue<bool>())
                {
                    if (PlayerH.GetItemDamage(target, Damage.DamageItems.Botrk) + Player.Health <= Player.MaxHealth)
                    {
                        BladeItem.Cast(target);
                    }
                }
                else
                {
                    BladeItem.Cast(target);
                }
            }
            if(QbMenu.Item("mura").GetValue<bool>() && MuraItem.IsReady() && !Player.HasBuff("Muramana") && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                MuraItem.Cast();
            }

            if (castE && target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.CastIfHitchanceEquals(target, HitChance.Medium);
            }

            if (castW && !QbMenu.Item("useSW").GetValue<bool>() && castSw && W.IsReady())
            {
                W.Cast();
            }

            if (castQ && target.IsValidTarget(Q2.Range) && target.HasBuff("urgotcorrosivedebuff", true) && Q2.IsReady())
            {
                if (castW && W.IsReady())
                {
                    W.Cast();
                }
                Q2.Cast(target.ServerPosition);
            }

            if (castQ && target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target.ServerPosition);
            }

            

        }

        public static void Harass()
        {
            var target = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Physical);
            if (target == null || Player.Mana <= Player.MaxMana * (QbMenu.Item("HarassMana").GetValue<int>() / 100.0)) return;
            var castQ = QbMenu.Item("useHQ").GetValue<bool>();
            var castE = QbMenu.Item("useHE").GetValue<bool>();
            var castW = QbMenu.Item("useHW").GetValue<bool>();

            if (castE && target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.CastIfHitchanceEquals(target, HitChance.Medium);
            }

            if (castQ && target.IsValidTarget(Q2.Range) && target.HasBuff("urgotcorrosivedebuff", true) && Q2.IsReady())
            {
                if (castW && W.IsReady())
                {
                    W.Cast();
                }

                Q2.Cast(target.ServerPosition);

            }
            else
            {
                if (castQ && target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.Cast(target.ServerPosition);
                }
            }
        }

        public static void Laneclear()
        {
            if (!Orbwalking.CanMove(40) || Player.Mana <= Player.MaxMana * (QbMenu.Item("LaneMana").GetValue<int>() / 100.0)) return;
            var myMinions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange);
            var castQ = QbMenu.Item("useLQ").GetValue<bool>();
            var castE = QbMenu.Item("useLE").GetValue<bool>();

            if (castE && E.IsReady())
            {
                foreach (var minion in myMinions.Where(minion => minion.IsValidTarget()))
                {
                    if (minion.IsValidTarget(E.Range))
                    {
                        E.Cast(minion);
                    }
                }
            }

            if (castQ && Q.IsReady())
            {
                foreach (var minion in myMinions.Where(minion => minion.IsValidTarget()))
                {
                    if (Vector3.Distance(minion.ServerPosition, Player.ServerPosition) <= Q2.Range && minion.HasBuff("urgotcorrosivedebuff", true))
                    {
                        Q2.Cast(minion.ServerPosition);
                    }
                    if (Vector3.Distance(minion.ServerPosition, Player.ServerPosition) <= Q.Range)
                    {
                        Q.Cast(minion.ServerPosition);
                    }
                }
            }

        }

        public static void Lasthit()
        {
            if (!Orbwalking.CanMove(40) || Player.Mana <= Player.MaxMana * (QbMenu.Item("LastMana").GetValue<int>() / 100.0)) return;
            var myMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var castQ = QbMenu.Item("useLhQ").GetValue<bool>();

            if (castQ && Q.IsReady())
            {
                foreach (var minion in myMinions.Where(minion => PlayerH.GetSpellDamage(minion,SpellSlot.Q) >= HealthPrediction.GetHealthPrediction(minion, (int)(Q.Delay * 1000))))
                {
                        Q.Cast(minion.ServerPosition);
                }
            }
        }

    }
}
