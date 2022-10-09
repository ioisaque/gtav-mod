using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class Win32
{
    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    [DllImport("User32.Dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }
}

public class block : Script
{
    // Inicialização de variáveis globais.

    // Player
    private Ped playerPed = Game.Player.Character;
    private Vector3 playerPos = Game.Player.Character.Position;

    bool isGodMode = Game.Player.IsInvincible;
    bool isNeverWanted = false;
    bool isSuperJump = false;
    int WantedLevel = Game.Player.WantedLevel;

    Ped pTarget = null;

    // ESP
    bool isEspDist = false;
    bool isEspSkeleton = false;
    bool isEspLine = false;
    bool isEspTarget = false;

    // Others
    bool isBoneDebug = false;
    bool isMagnetic = false;
    bool isCopSpawner = false;
    Vector3 SpawnPoint = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 4.0f;

    bool isNoReload = false;
    bool isInfinityAmmo = false;

    long old_tick = Environment.TickCount;

    // HotKeys
    bool isControlPressed = false;

    // Menu
    MenuPool modMenuPool;

    UIMenu mainMenu;

    UIMenu playerSection;
    UIMenu weaponSection;
    UIMenu vehicleSection;

    UIMenu HackingSection;
    UIMenu EspSection;

    // Database
    List<dynamic> listOfWeapons = new List<dynamic>();
    WeaponHash[] allWeaponsHash = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));

    List<dynamic> listOfVehicles = new List<dynamic>();
    VehicleHash[] allVehiclesHash = (VehicleHash[])Enum.GetValues(typeof(VehicleHash));

    List<dynamic> listOfPeds = new List<dynamic>();

    List<dynamic> listOfBones = new List<dynamic>();
    Bone[] allBones = (Bone[])Enum.GetValues(typeof(Bone));

    // Cria o menu principal, submenus e chama as funções que criam os items/funções.
    private void setupMenu()
    {
        modMenuPool = new MenuPool();
        mainMenu = new UIMenu("Scripts by Block", "v1.0");

        modMenuPool.Add(mainMenu);
        playerSection = modMenuPool.AddSubMenu(mainMenu, "Player");
        weaponSection = modMenuPool.AddSubMenu(mainMenu, "Armas");
        vehicleSection = modMenuPool.AddSubMenu(mainMenu, "Veículos");

        HackingSection = modMenuPool.AddSubMenu(mainMenu, "Hacking");
        EspSection     = modMenuPool.AddSubMenu(HackingSection, "ESP");

        MainFunctions();

        PlayerFunctions();
        WeaponFunctions();
        VehicleFunctions();

        HackingFunctions();
    }

    // Cria o banco de dados de armas, veículos, etc...
    private void setupDataBase()
    {
        for (int i = 0; i < allWeaponsHash.Length; i++)
          listOfWeapons.Add(allWeaponsHash[i]);

        for (int i = 0; i < allVehiclesHash.Length; i++)
            listOfVehicles.Add(allVehiclesHash[i]);

        for (int i = 0; i < allBones.Length; i++)
            listOfBones.Add(allBones[i] + " ID: " + i);

        //listOfBones.Add("Name: " + allBones[i] + " ID: " + Function.Call(Hash.GET_PED_BONE_INDEX, );
    }

    // Onde se inicializa eventos ou qualquer outra coisa no momento que o mod carrega.
    public block()
    {
        setupDataBase();
        setupMenu();
        UI.Notify("Olá " + Game.Player.Name + ", preparado?\n\nÉ hora do show PORRA!\nBIIIIIRRRRRLLLL");
        UI.Notify("Scripts by Block ativos!");

        Tick += OnTick;
        KeyUp += OnKeyUp;
        KeyDown += OnKeyDown;
    }

    void MainFunctions()
    {
        UIMenuItem spaceLine = new UIMenuItem("");
        mainMenu.AddItem(spaceLine);

        UIMenuItem killAll = new UIMenuItem("Kill All.");
        mainMenu.AddItem(killAll);

        UIMenuItem wantAttention = new UIMenuItem("Quero Atenção...");
        mainMenu.AddItem(wantAttention);

        mainMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == killAll)
                foreach (Ped p in World.GetAllPeds())
                    if (p != playerPed)
                        p.Kill();

           if (item == wantAttention)
                Game.Player.WantedLevel = 5;
        };
    }

    void PlayerFunctions()
    {
        UIMenuItem RestorePlayerStatus = new UIMenuItem("Restaurar TUDO.");
        playerSection.AddItem(RestorePlayerStatus);

        UIMenuCheckboxItem GodMode = new UIMenuCheckboxItem("God Mode",isGodMode);
        playerSection.AddItem(GodMode);

        UIMenuCheckboxItem NeverWanted = new UIMenuCheckboxItem("Nunca Procurado", isNeverWanted);
        playerSection.AddItem(NeverWanted);

        UIMenuCheckboxItem SuperJump = new UIMenuCheckboxItem("Super Jump", isSuperJump);
        playerSection.AddItem(SuperJump);

        UIMenuItem spawnBodyGuard = new UIMenuItem("Spawnar guarda costas.");
        playerSection.AddItem(spawnBodyGuard);

        playerSection.OnCheckboxChange += (sender, item, index) =>
        {
            if (item == GodMode)
            {
                isGodMode = !isGodMode;

                if (isGodMode)
                {
                    Game.Player.IsInvincible = true;
                    UI.Notify("Gomen'nasai Zen'O sama!!! ");
                }
                else
                    Game.Player.IsInvincible = false;
            }
            if (item == NeverWanted)
            {
                isNeverWanted = !isNeverWanted;

                if (isNeverWanted)
                    UI.Notify("Opa Deputado, tudo certo?");
            }
            if (item == SuperJump)
            {
                isSuperJump = !isSuperJump;

                if (isSuperJump)
                    UI.Notify("É um pássaro? É um avião?\nNão! É um tal de Jão!");
            }
        };

        playerSection.OnItemSelect += (sender, item, index) =>
        {
            if (item == RestorePlayerStatus)
            {
                Game.Player.Money += 2500;
                Game.Player.Character.Armor = Game.Player.MaxArmor;
                Game.Player.Character.Health = Game.Player.Character.MaxHealth;

                if (playerPed.IsInVehicle())
                {
                    Vehicle currentV = playerPed.CurrentVehicle;

                    currentV.Repair();
                    currentV.Wash();
                }

                for (int i = 0; i < allWeaponsHash.Length; i++)
                {
                    WeaponHash currentHash = allWeaponsHash[i];

                    //if (Game.Player.Character.Weapons.HasWeapon(currentHash))
                      // A fazer... Restaurar munição...
                }
            }
            if (item == spawnBodyGuard)
            {
                Ped BodyGuard = World.CreatePed("CSB_MWEATHER", playerPos + Game.Player.Character.ForwardVector * 4.0f);

                for (int i = 0; i < allWeaponsHash.Length; i++)
                {
                    WeaponHash currentHash = allWeaponsHash[i];
                    BodyGuard.Weapons.Give((WeaponHash)Function.Call<int>(Hash.GET_HASH_KEY, "WEAPON_ASSAULTRIFLE"), 9999, true, false);
                }

                Game.Player.Character.CurrentPedGroup.Add(BodyGuard, false);

                UI.Notify("Fica sussa meu chapa!");
            }
        };

    }
    
    void WeaponFunctions()
    {
        UIMenuListItem list = new UIMenuListItem("Adicionar uma da lista: ", listOfWeapons, 0);
        weaponSection.AddItem(list);

        UIMenuItem getAllWeapons = new UIMenuItem("Adicionar todas as armas.");
        weaponSection.AddItem(getAllWeapons);

        UIMenuCheckboxItem noReload = new UIMenuCheckboxItem("No Reload", isNoReload);
        weaponSection.AddItem(noReload);

        UIMenuCheckboxItem infinityAmmo = new UIMenuCheckboxItem("Munição Infinita", isInfinityAmmo);
        weaponSection.AddItem(infinityAmmo);

        weaponSection.OnItemSelect += (sender, item, index) =>
        {
            if (item == list)
            {
                int listIndex = list.Index;
                WeaponHash currentHash = allWeaponsHash[listIndex];

                Game.Player.Character.Weapons.Give(currentHash, 480, true, true);

                UI.Notify(allWeaponsHash[listIndex] + " entregue!");
            }

            if (item == getAllWeapons)
            {
                for (int i = 0; i < allWeaponsHash.Length; i++)
                {
                    WeaponHash currentHash = allWeaponsHash[i];
                    Game.Player.Character.Weapons.HasWeapon(currentHash);
                    

                    Game.Player.Character.Weapons.Give(currentHash, 480, true, true);
                }

                UI.Notify("Rambo mode ON! xD");
            }
        };

        weaponSection.OnCheckboxChange += (sender, item, index) =>
        {
            if (item == noReload)
            {
                isNoReload = !isNoReload;

                if (isNoReload)
                    UI.Notify("Welcome to Hollywood!");
                else
                    UI.Notify("De volta a realidade é?\nB O R I N G  ALERT!");
            }

            if (item == infinityAmmo)
            {
                isInfinityAmmo = !isInfinityAmmo;

                if (isInfinityAmmo)
                    UI.Notify("Paia... No Reload > all.");
            }
        };
    }

    void VehicleFunctions()
    {
        UIMenuItem FixVehicle = new UIMenuItem("Dar uma geral no veículo atual.");
        vehicleSection.AddItem(FixVehicle);

        UIMenuListItem VehicleSelector = new UIMenuListItem("Spawn da lista: ", listOfVehicles, 0);
        vehicleSection.AddItem(VehicleSelector);

        UIMenuItem SpawnByName = new UIMenuItem("Inserir nome para Spawn.");
        vehicleSection.AddItem(SpawnByName);

        UIMenuItem DeleteAll = new UIMenuItem("Deletar todos os veículos.");
        vehicleSection.AddItem(DeleteAll);

        vehicleSection.OnItemSelect += (sender, item, index) =>
        {

            if (item == FixVehicle)
            {
                if (playerPed.IsInVehicle())
                {
                    Vehicle currentV = playerPed.CurrentVehicle;

                    currentV.Repair();
                    currentV.Wash();

                    UI.Notify("De carro novo é? xD");
                }
            }

            if (item == VehicleSelector)
            {
                int listIndex = VehicleSelector.Index;
                VehicleHash currentHash = allVehiclesHash[listIndex];
                Vehicle v = World.CreateVehicle(currentHash, playerPed.Position, playerPed.Heading);

                v.NumberPlate = "SPAWNED";

                v.PlaceOnGround();
                playerPed.Task.WarpIntoVehicle(v, VehicleSeat.Driver);

                UI.Notify(allVehiclesHash[listIndex] + " entregue!");
            }

            if (item == SpawnByName)
            {
                string modelName = Game.GetUserInput(50);
                Model model = new Model(modelName);
                model.Request();

                if (model.IsInCdImage && model.IsValid)
                {
                    Vehicle v = World.CreateVehicle(model, playerPed.Position, playerPed.Heading);

                    v.NumberPlate = "SPAWNED";
                    //v.EngineRunning = true;
                    //v.PrimaryColor = VehicleColor.MetallicBlue;
                    //v.SecondaryColor = VehicleColor.MetallicBlue;

                    //v.PlaceOnGround();

                    v.PlaceOnGround();
                    playerPed.Task.WarpIntoVehicle(v, VehicleSeat.Driver);

                    UI.Notify(modelName + " entregue!");
                }
            }

            if (item == DeleteAll)
            {
                Vehicle currentV = null;

                if (playerPed.IsInVehicle())
                    currentV = playerPed.CurrentVehicle;

                foreach (Vehicle v in World.GetAllVehicles())
                   if (v != currentV)
                    v.Delete();
            }
        };
    }

    void HackingFunctions()
    {
        //General
        UIMenuCheckboxItem boneslist = new UIMenuCheckboxItem("Debug Bones", isBoneDebug);
        HackingSection.AddItem(boneslist);

        UIMenuCheckboxItem spawnEnemy = new UIMenuCheckboxItem("Spawn Marines", isCopSpawner);
        HackingSection.AddItem(spawnEnemy);

        UIMenuCheckboxItem Magnetic = new UIMenuCheckboxItem("Magnetic", isMagnetic);
        HackingSection.AddItem(Magnetic);

        HackingSection.OnCheckboxChange += (sender, item, index) =>
        {
            if (item == boneslist)
                isBoneDebug = !isBoneDebug;

            if (item == spawnEnemy)
            {
                isCopSpawner = !isCopSpawner;
                SpawnPoint = playerPos + Game.Player.Character.ForwardVector * 3.0f;
            }

            if (item == Magnetic)
                isMagnetic = !isMagnetic;
        };

        //ESP
        UIMenuCheckboxItem drawEspName = new UIMenuCheckboxItem("ESP Names", isEspDist);
        EspSection.AddItem(drawEspName);

        UIMenuCheckboxItem drawEspSkeleton = new UIMenuCheckboxItem("ESP Skeleton", isEspSkeleton);
        EspSection.AddItem(drawEspSkeleton);

        UIMenuCheckboxItem drawEspLine = new UIMenuCheckboxItem("ESP Lines", isEspLine);
        EspSection.AddItem(drawEspLine);

        UIMenuCheckboxItem drawTargetMark = new UIMenuCheckboxItem("Target Mark", isEspTarget);
        EspSection.AddItem(drawTargetMark);

        EspSection.OnCheckboxChange += (sender, item, index) =>
        {
            if (item == drawEspName)
                isEspDist = !isEspDist;

            if (item == drawEspSkeleton)
                isEspSkeleton = !isEspSkeleton;

            if (item == drawEspLine)
                isEspLine = !isEspLine;

            if (item == drawTargetMark)
                isEspTarget = !isEspTarget;
        };
    }

    void Magnetic()
    {
        Vector3 MagSpot = playerPos + Game.Player.Character.ForwardVector * 3.5f;
        MagSpot.Z = MagSpot.Z - 1;

        foreach (Ped p in World.GetAllPeds())
            if (p != playerPed && !p.IsDead)
            {
                p.Position = MagSpot;
                p.Heading = playerPed.Heading;
                p.Task.StandStill(1000);
            }
       
    }

    bool PedIsValid(Ped p)
    {
        if (!p.IsPlayer && p.IsOnScreen && p.IsAlive && p.IsVisible)
            return true;

        return false;
    }

    // Loop
    private void OnTick(object sender, EventArgs e)
    {
        if (modMenuPool != null)
            modMenuPool.ProcessMenus();

        if (playerPed.IsAlive)
            playerPos = playerPed.Position;

        GetNearestPedFrom(playerPed);

        if (isEspTarget)
        {
            Vector3 tPos = pTarget.GetBoneCoord(allBones[20]);
            tPos.Z = tPos.Z + 1;
            Point fPos = WorldToScreenPoint(tPos);
            fPos.X = fPos.X - 30;

            UIText debugText = new UIText("<< Target Ped >>", fPos, 1)
            {
                Font = GTA.Font.ChaletLondon,
                Scale = 0.3f,
                Color = Color.Gold,
                Outline = true
            };

            debugText.Draw();
        }

        if (isEspDist)
        {
            foreach (Ped p in World.GetAllPeds())
                if (PedIsValid(p))
                {
                    Vector3 tPos = p.GetBoneCoord(allBones[21]);
                    Point fPos = WorldToScreenPoint(tPos);
                    fPos.X = fPos.X - 30;

                    float dist = Vector3.Distance(playerPed.Position, tPos) - 1;

                    UIText debugText = new UIText("Ped " + dist.ToString("0.0") + " mts", fPos, 1)
                    {
                        Font = GTA.Font.ChaletLondon,
                        Scale = 0.3f,
                        Color = Color.White,
                        Outline = true
                    };

                    if (dist < 100)
                        debugText.Draw();
                }
        }

        if (isEspSkeleton)
        {
            Color ESPColor = Color.FromArgb(255, 0, 255, 0);

            foreach (Ped p in World.GetAllPeds())
                if (PedIsValid(p))
                {
                    //Perna Esquerda
                    BoneLineTo(p, allBones[21], allBones[84], ESPColor);
                    BoneLineTo(p, allBones[84], allBones[73], ESPColor);
                    //Perna Direita
                    BoneLineTo(p, allBones[67], allBones[53], ESPColor);
                    BoneLineTo(p, allBones[53], allBones[68], ESPColor);
                    //Torso
                    BoneLineTo(p, allBones[73], allBones[33], ESPColor);
                    BoneLineTo(p, allBones[67], allBones[33], ESPColor);
                    BoneLineTo(p, allBones[33], allBones[56], ESPColor);
                    //Ombros
                    BoneLineTo(p, allBones[56], allBones[57], ESPColor);
                    BoneLineTo(p, allBones[56], allBones[60], ESPColor);
                    //Braço Esquerdo
                    BoneLineTo(p, allBones[60], allBones[82], ESPColor);
                    BoneLineTo(p, allBones[82], allBones[25], ESPColor);
                    //Braço Direito
                    BoneLineTo(p, allBones[57], allBones[46], ESPColor);
                    BoneLineTo(p, allBones[46], allBones[70], ESPColor);
                }
        }

        if (isEspLine)
        {
            Color ESPColor = Color.Yellow;//Color.FromArgb(255, 255, 87, 45);
            if (pTarget.IsOnScreen)
            {
                BoneLine(allBones[21], ESPColor);
                BoneLine(allBones[25], ESPColor);
                BoneLine(allBones[33], ESPColor);
                BoneLine(allBones[46], ESPColor);
                BoneLine(allBones[53], ESPColor);
                BoneLine(allBones[56], ESPColor);
                BoneLine(allBones[57], ESPColor);
                BoneLine(allBones[60], ESPColor);
                BoneLine(allBones[67], ESPColor);
                BoneLine(allBones[68], ESPColor);
                BoneLine(allBones[70], ESPColor);
                BoneLine(allBones[73], ESPColor);
                BoneLine(allBones[82], ESPColor);
                BoneLine(allBones[84], ESPColor);

            }
        }

        if (isBoneDebug)
        {
            for (int i = 0; i < allBones.Length; i++)
                if (PedIsValid(pTarget))
                {
                    Vector3 bPos = pTarget.GetBoneCoord(allBones[i]);
                    Point fPos = WorldToScreenPoint(bPos);
                    fPos.X = fPos.X - 35;

                    string boneName = listOfBones[i];

                    if (boneName.Contains("SKEL_") && !boneName.Contains("Finger"))
                    {
                        string newName = boneName.Replace("SKEL_", "");

                        UIText debugText = new UIText(newName, fPos, 1)
                        {
                            Font = GTA.Font.ChaletLondon,
                            Scale = 0.25f,
                            Color = Color.Lime,
                            Outline = true
                        };

                        debugText.Draw();
                    }

                }
        }

        if (isSuperJump)
            Game.Player.SetSuperJumpThisFrame();

        if (isNeverWanted)
            Game.Player.WantedLevel = 0;

        if (isNoReload)
            playerPed.Weapons.Current.InfiniteAmmoClip = true;
        else
            playerPed.Weapons.Current.InfiniteAmmoClip = false;

        if (isInfinityAmmo)
            playerPed.Weapons.Current.InfiniteAmmo = true;
        else
            playerPed.Weapons.Current.InfiniteAmmo = false;

        if (isMagnetic)
            Magnetic();

        if ((isCopSpawner) && (Environment.TickCount - old_tick > 1000))
        {
            Ped cop = World.CreatePed("S_M_Y_MARINE_01", SpawnPoint);
            cop.IsEnemy = true;

            cop.Weapons.Give((WeaponHash)Function.Call<int>(Hash.GET_HASH_KEY, "WEAPON_ASSAULTRIFLE"), 500, true, false);

            old_tick = Environment.TickCount;
        }
    }

    // Quando uma tecla é pressionada ou segurada.
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ControlKey)
            isControlPressed = true;

        if (e.KeyCode == Keys.F10 && !modMenuPool.IsAnyMenuOpen())
            mainMenu.Visible = !mainMenu.Visible;

        if (e.KeyCode == Keys.Delete)
            isMagnetic = !isMagnetic;
    }

    // Quando uma tecla é solta.
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.ControlKey)
            isControlPressed = false;
    }

    public Vector2 WorldToScreen(Vector3 W3D)
    {
        Point sPos = UI.WorldToScreen(W3D);
        var res = UIMenu.GetScreenResolutionMantainRatio();

        Vector2 S2D = new Vector2((float)((sPos.X / (float)UI.WIDTH) * res.Width),
                         (float)((sPos.Y / (float)UI.HEIGHT) * res.Height));
        return S2D;
    }

    public Point WorldToScreenPoint(Vector3 W3D)
    {
        Point sPos = UI.WorldToScreen(W3D);
        Point S2D = new Point((int)((sPos.X / (float)UI.WIDTH) * 1360), (int)((sPos.Y / (float)UI.HEIGHT) * 720));

        return S2D;
    }

    void BoneLine(Bone vBone, Color col)
    {
        Vector3 sPos = playerPed.GetBoneCoord(vBone);
        Vector3 ePos = pTarget.GetBoneCoord(vBone);

        Function.Call(Hash.DRAW_LINE, sPos.X, sPos.Y, sPos.Z, ePos.X, ePos.Y, ePos.Z, col.R, col.G, col.B, col.A);
    }

    void BoneLineTo(Ped p, Bone sBone, Bone eBone, Color col)
    {
        Vector3 sPos = p.GetBoneCoord(sBone);
        sPos.X = sPos.X + 0.1f;
        sPos.Y = sPos.Y + 0.1f;

        Vector3 ePos = p.GetBoneCoord(eBone);
        ePos.X = ePos.X + 0.1f;
        ePos.Y = ePos.Y + 0.1f;

        Function.Call(Hash.DRAW_LINE, sPos.X, sPos.Y, sPos.Z, ePos.X, ePos.Y, ePos.Z, col.R, col.G, col.B, col.A);
    }

    void GetNearestPedFrom(Ped pFrom)
    {
        float last_ped_dist = 9999;
        Vector3 pPos = Vector3.RandomXYZ();
        float dist = Vector3.Distance(pFrom.Position, pPos);

        foreach (Ped p in World.GetAllPeds())
        {
            if (p.IsOnScreen && PedIsValid(p))
            {
                pPos = p.Position;
                dist = Vector3.Distance(pFrom.Position, pPos);

                if ((dist > 0) && (dist < last_ped_dist))
                {
                    pTarget = p;
                    last_ped_dist = Vector3.Distance(pFrom.Position, pPos);
                }
            }
        }
    }

    // Funções em desuso...
    string GetStatusText(bool fStatus)
    {
        if (fStatus)
            return ":   Ligado";
        else
            return ":   Desligado";
    }
}