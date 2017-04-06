using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace UnsignedAnnie
{
    internal class Program
    {
        public static Menu ComboMenu, DrawingsMenu, SettingsMenu, LaneClear, LastHit, Killsteal, Harass, menu;
        public static Spell.Targeted Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static int PassiveStacks
        {
            get
            {
                int stacks = 0;
                if(_Player.HasBuff("pyromania"))
                    stacks = _Player.GetBuff("pyromania").Count;
                return stacks;
            } 
        }
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        public static int Mana { get { return (int)_Player.Mana; } }
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Annie")
                return;

            //Hacks.AntiAFK = true;
            Bootstrap.Init(null);

            Q = new Spell.Targeted(SpellSlot.Q, 625, DamageType.Magical);
            W = new Spell.Skillshot(SpellSlot.W, 625, SkillShotType.Cone, 250, null, null, DamageType.Mixed)
            {
                ConeAngleDegrees = 50
            };
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 0, int.MaxValue, 290, DamageType.Magical);

            menu = MainMenu.AddMenu("Unsigned Annie", "UnsignedAnnie");

            ComboMenu = menu.AddSubMenu("Kombo", "combomenu");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("Q", new CheckBox("q yu kullan"));
            ComboMenu.Add("W", new CheckBox("w  kullan"));
            ComboMenu.Add("E", new CheckBox("E kullan"));
            ComboMenu.Add("R", new CheckBox("R kullan"));
            //ComboMenu.Add("Flash Ult", new CheckBox("Flash R"));
            //ComboMenu.Add("Flash Ult People", new Slider("Flash R at x People: ", 4, 0, 5));
            ComboMenu.Add("Items", new CheckBox("Zhonya kullan"));
            ComboMenu.Add("Ignite", new CheckBox("Tutuştur kullan"));

            LaneClear = menu.AddSubMenu("Lane ittirme", "laneclear");
            LaneClear.AddGroupLabel("Lane Clear Settings");
            LaneClear.Add("Q", new CheckBox("Use Q"));
            LaneClear.Add("QForLastHit", new CheckBox("Q ile son vuruş yap"));
            LaneClear.Add("W", new CheckBox("w yu kullan"));

            Harass = menu.AddSubMenu("Harass", "harass");
            Harass.AddGroupLabel("Harass Settings");
            Harass.Add("Q", new CheckBox("Q yu kullan"));
            Harass.Add("W", new CheckBox("W yu kullan"));

            LastHit = menu.AddSubMenu("Son Vuruş", "lasthitmenu");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("Q", new CheckBox("Q Kullan"));
            LastHit.Add("W", new CheckBox("W yu kullan", false));

            Killsteal = menu.AddSubMenu("Killsteal", "killstealmenu");
            Killsteal.AddGroupLabel("Killsteal Settings");
            Killsteal.Add("KS", new CheckBox("Activate Killsteal"));
            Killsteal.Add("Q", new CheckBox("Q kullan"));
            Killsteal.Add("W", new CheckBox("W yu kullan"));
            Killsteal.Add("R", new CheckBox("R kullan", false));
            Killsteal.Add("Ignite", new CheckBox("Tutuştur kullan"));

            DrawingsMenu = menu.AddSubMenu("Drawings", "drawingsmenu");
            DrawingsMenu.AddGroupLabel("Drawings Settings");
            DrawingsMenu.Add("Q", new CheckBox("q ve W çizimi"));
            DrawingsMenu.Add("R", new CheckBox("R cizimi"));
            
            SettingsMenu = menu.AddSubMenu("Ayarlar", "settingsmenu");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.Add("Stack", new CheckBox("Base icinde stun biriktir"));
            SettingsMenu.Add("Health Potions", new CheckBox("otamatik kırmızı pot kullan"));
            SettingsMenu.Add("Tibbers Controller", new CheckBox("Otoatik Tibersı cagır"));
            SettingsMenu.Add("Auto R", new CheckBox("tibers ile kombo yap(stun biriktir)"));

            SpellDataInst Sum1 = _Player.Spellbook.GetSpell(SpellSlot.Summoner1);
            SpellDataInst Sum2 = _Player.Spellbook.GetSpell(SpellSlot.Summoner2);
            if (Sum1.Name == "summonerdot")
                Ignite = new Spell.Targeted(SpellSlot.Summoner1, 600);
            else if (Sum2.Name == "summonerdot")
                Ignite = new Spell.Targeted(SpellSlot.Summoner2, 600);
            
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingsMenu["Q"].Cast<CheckBox>().CurrentValue && (Q.IsLearned || W.IsLearned))
                Drawing.DrawCircle(_Player.Position, Q.Range, System.Drawing.Color.BlueViolet);
            if (DrawingsMenu["R"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                Drawing.DrawCircle(_Player.Position, R.Range + (R.Width / 2), System.Drawing.Color.BlueViolet);
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                AnnieFunctions.Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                AnnieFunctions.LastHit();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                AnnieFunctions.Harrass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                AnnieFunctions.LaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                AnnieFunctions.Flee();
            if (SettingsMenu["Stack"].Cast<CheckBox>().CurrentValue)
                AnnieFunctions.StackMode();
            if (Killsteal["KS"].Cast<CheckBox>().CurrentValue)
                AnnieFunctions.KillSteal();
            if (SettingsMenu["Health Potions"].Cast<CheckBox>().CurrentValue)
                AnnieFunctions.UseItems();
            if (SettingsMenu["Tibbers Controller"].Cast<CheckBox>().CurrentValue)
                AnnieFunctions.ControlTibbers();
            if (SettingsMenu["Auto R"].Cast<CheckBox>().CurrentValue)
                AnnieFunctions.AutoUlt();
        }
    }
}
