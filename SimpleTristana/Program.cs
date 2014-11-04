using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleTristana
{
    class Program
    {
        
        public static string ChampName = "Tristana";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Obj_AI_Hero PlayerH = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Items.Item DfgItem = new Items.Item(3128, 750);
        public static Items.Item BladeItem = new Items.Item(3153, 450);
        public static Items.Item GhostItem = new Items.Item(3142, float.MaxValue);

        public static Menu QbMenu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 541);
            R = new Spell(SpellSlot.R, 541);
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
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E").SetValue(false));
            QbMenu.SubMenu("Combo").AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Harras menu
            QbMenu.AddSubMenu(new Menu("Harras", "Harras"));
            QbMenu.SubMenu("Harras").AddItem(new MenuItem("useHQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Harras").AddItem(new MenuItem("useHE", "Use E").SetValue(false));
            QbMenu.SubMenu("Harras").AddItem(new MenuItem("HarrasKey", "Harras Key").SetValue(new KeyBind(67, KeyBindType.Press)));
            QbMenu.SubMenu("Harras").AddItem(new MenuItem("HarrasKey2", "Harras Key 2").SetValue(new KeyBind(86, KeyBindType.Press)));
            //LaneClear
            QbMenu.AddSubMenu(new Menu("Laneclear", "Laneclear"));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("useLQ", "Use Q").SetValue(false));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("useLE", "Use E").SetValue(false));
            QbMenu.SubMenu("Laneclear").AddItem(new MenuItem("LaneclearKey", "Laneclear Key").SetValue(new KeyBind(86, KeyBindType.Press)));
            //Ultimate & KS
            QbMenu.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("autoR", "Auto Ultimate").SetValue(false));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("FullE", "Full E").SetValue(false));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("SingleETick", "Few E Ticks").SetValue(false));
            QbMenu.SubMenu("Killsteal").AddItem(new MenuItem("SingleESlider", "How much Ticks").SetValue(new Slider(4, 1, 5)));
            //Items
            QbMenu.AddSubMenu(new Menu("Items", "Items"));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("botrk", "Blade of the Ruined King").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("bomh", "Wait for max heal with blade").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("ghostbl", "Ghostblade").SetValue(false));
            QbMenu.SubMenu("Items").AddItem(new MenuItem("usedfg", "Deathfire Grasp").SetValue(false));
            //Misc
            QbMenu.AddSubMenu(new Menu("Misc", "Misc"));
            QbMenu.SubMenu("Misc").AddItem(new MenuItem("APMode", "Combo E > Q").SetValue(false));
            //Exploits
            QbMenu.AddItem(new MenuItem("NFD", "No Face Direction Exploit").SetValue(false));
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
            if (QbMenu.Item("HarrasKey").GetValue<KeyBind>().Active || QbMenu.Item("HarrasKey2").GetValue<KeyBind>().Active)
            {
                Harras();
            }
            if (QbMenu.Item("LaneclearKey").GetValue<KeyBind>().Active)
            {
                Laneclear();
            }
            if (QbMenu.Item("autoR").GetValue<bool>())
            {
                AutoR();
            }
            if (QbMenu.Item("SingleETick").GetValue<bool>())
            {
                SingleE();
            }
            if (QbMenu.Item("FullE").GetValue<bool>())
            {
                FullE();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {

        }

        public static void SingleE()
        {
            var target = SimpleTs.GetTarget(Player.AttackRange, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (E.IsReady() && target.Health + (target.HPRegenRate / 5 * 3) + 50 < (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) / 5 * QbMenu.Item("SingleESlider").GetValue<Slider>().Value))
                E.Cast(target, QbMenu.Item("NFD").GetValue<bool>());
        }

        public static void FullE()
        {
            var unit = ObjectManager.Get<Obj_AI_Hero>().First(obj => obj.IsValidTarget(600) && E.IsKillable(obj));
            if (unit != null)
            {
                E.Cast(unit);
            }
        }

        public static void AutoR()
        {
            var unit = ObjectManager.Get<Obj_AI_Hero>().First(obj => obj.IsValidTarget(600) && R.IsKillable(obj));
            if (unit != null)
            {
                R.Cast(unit);
            }
        }

        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Player.AttackRange, SimpleTs.DamageType.Physical);
            if (target == null) return;
            var castQ = QbMenu.Item("useQ").GetValue<bool>();
            var castE = QbMenu.Item("useE").GetValue<bool>();
            var test = PlayerH.GetItemDamage(target, Damage.DamageItems.Botrk);

            if (QbMenu.Item("ghostbl").GetValue<bool>() && GhostItem.IsReady() && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                GhostItem.Cast();
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

            if (QbMenu.Item("usedfg").GetValue<bool>() && DfgItem.IsReady() && target.IsValidTarget(DfgItem.Range))
            {
                DfgItem.Cast(target);
            }

            if (QbMenu.Item("APMode").GetValue<bool>())
            {
                if (castE && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && E.IsReady())
                {
                    E.Cast(target, QbMenu.Item("NFD").GetValue<bool>());
                }

                if (castQ && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && Q.IsReady())
                {
                    Q.Cast();
                }
            }
            else
            {
                if (castQ && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && Q.IsReady())
                {
                    Q.Cast();
                }

                if (castE && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && E.IsReady())
                {
                    E.Cast(target, QbMenu.Item("NFD").GetValue<bool>());
                }
            }
        }

        public static void Harras()
        {
            var target = SimpleTs.GetTarget(Player.AttackRange, SimpleTs.DamageType.Physical);
            if (target == null) return;
            var castQ = QbMenu.Item("useHQ").GetValue<bool>();
            var castE = QbMenu.Item("useHE").GetValue<bool>();
            if (castQ && target.IsValidTarget() && Q.IsReady())
            {
                Q.Cast();
            }
            if (castE && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && E.IsReady())
            {
                E.Cast(target, QbMenu.Item("NFD").GetValue<bool>());
            }
        }

        public static void Laneclear()
        {
            if (!Orbwalking.CanMove(40)) return;
            var myMinions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange);
            var castQ = QbMenu.Item("useLQ").GetValue<bool>();
            var castE = QbMenu.Item("useLE").GetValue<bool>();
            if (castQ && Q.IsReady())
            {
                foreach (var minion in myMinions.Where(minion => minion.IsValidTarget()))
                {
                    if (Vector3.Distance(minion.ServerPosition, Player.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player))
                    {
                        Q.Cast();

                    }
                }
            }

            if (castE && E.IsReady())
            {
                foreach (var minion in myMinions.Where(minion => minion.IsValidTarget()))
                {
                    if (minion.IsValidTarget(Player.AttackRange))
                    {
                        E.Cast(minion, true);
                    }
                }
            }
        }



    }
}
