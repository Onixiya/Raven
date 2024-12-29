[assembly:MelonGame("Ninja Kiwi","BloonsTD6")]
[assembly:MelonInfo(typeof(Raven.SC2ModMain),Raven.ModHelperData.Name,Raven.ModHelperData.Version,"Silentstorm")]
namespace Raven{
    public class SC2ModMain:MelonMod{
    }
    public class Raven:SC2Tower{
        public override string Name=>"Raven";
        public override Faction TowerFaction=>Faction.Terran;
        public override int MaxTier=>4;
        public override int Order=>1;
        public override Dictionary<string,Il2CppSystem.Type>Components=>new(){{"Raven-TurretGunPrefab",Il2CppType.Of<AutoTurret.AutoTurretCom>()}};
        public override UpgradeModel[]GenerateUpgradeModels(){
            return new UpgradeModel[]{
                new("Seeker Missile",900,0,new("Ui["+Name+"-SeekerMissileIcon]"),0,0,0,"","Seeker Missile"),
                new("Theia",2025,0,new("Ui["+Name+"-TheiaIcon]"),0,1,0,"","Theia"),
                new("Corvid Reactor",4150,0,new("Ui["+Name+"-CorvidReactorIcon]"),0,2,0,"","Corvid Reactor"),
                new("Science Vessel",11140,0,new("Ui["+Name+"-ScienceVesselIcon]"),0,3,0,"","Science Vessel")
            };
        }
        [HarmonyPatch(typeof(FilterInBaseTowerId),nameof(FilterInBaseTowerId.FilterTowerModel))]
        public class FilterInBaseTowerIdFilterTower_Patch{
            //__instance.entity.blahblahblah is the tower with the filter, towerModel is the tower thats being filtered out or in
            public static bool Prefix(FilterInBaseTowerId __instance,ref bool __result,TowerModel towerModel){
                if(__instance.entity.dependants.list[0].Cast<RangeSupport>().tower.towerModel.baseId!="Raven"){
                    if(__instance.filterInBaseTowerIdModel.baseIds.Contains(towerModel.baseId)){
                        __result=true;
                    }else{
                        __result=false;
                    }
                }else{
                    if(towerModel.baseId=="Raven"){
                        __result=false;
                    }else{
                        __result=true;
                    }
                }
                return false;
            }
        }
        //static field cannot contain the name field
        public static string BuffId="Ui[Raven-RangeBuffIcon]";
        public override ShopTowerDetailsModel ShopDetails(){
            ShopTowerDetailsModel details=gameModel.towerSet[0].Clone<ShopTowerDetailsModel>();
            details.towerId=Name;
            details.name=Name;
            details.towerIndex=14;
            details.pathOneMax=4;
            details.pathTwoMax=0;
            details.pathThreeMax=0;
            SerializableDictionary<string,BloonOverlayScriptable>overlays=GameData.Instance.bloonOverlays.overlayTypes;
            Il2CppSystem.Collections.Generic.List<Il2Cpp.BloonOverlayClass>glueKeys=overlays["GlueBasic"].assets.keys;
            BloonOverlayScriptable gas=ScriptableObject.CreateInstance<BloonOverlayScriptable>();
            gas.assets=new();
            for(int i=0;i<glueKeys.Count;i++){
                gas.assets.Add((Il2Cpp.BloonOverlayClass)glueKeys[i],new("Raven-GasPrefab"));
            }
            gas.displayLayer=1;
            gas.name="Raven-Gas";
            overlays.Add("Raven-Gas",gas);
            List<BuffIndicatorModel>buffs=gameModel.buffIndicatorModels.ToList();
            BuffIndicatorModel buff=new BuffIndicatorModel(Name+"RangeBuff",BuffId,BuffId,false,0,false);
            buffs.Add(buff);
            gameModel.buffIndicatorModels=buffs.ToArray();
            GameData.Instance.buffIconSprites.buffIconSprites.Add(new(){buffId=BuffId,icon=new(BuffId)});
			LocManager.textTable.Add(Name+" Description","Terran flying support. Buffs range and gives camo detection to nearby towers");
            LocManager.textTable.Add("Seeker Missile Description","Deploys fast and high damage missiles with target tracking");
            LocManager.textTable.Add("Theia Description","Advanced Dominion Raven. Places down automatic turrets nearby and increases range");
            LocManager.textTable.Add("Corvid Reactor Description","Upgraded Raven reactor, allows for much quicker Seeker Missile and Auto Turret production");
            LocManager.textTable.Add("Science Vessel Description","Manned science craft and predecessor to the Raven. Replaces Seeker Missile with Irradiate, dealing high damage to non Moabs");
            return details;
        }
        public override TowerModel[]GenerateTowerModels(){
            return new TowerModel[]{
                Base(),
                Seeker(),
                Theia(),
                Corvid(),
                Science()
            };
        }
        public TowerModel Base(){
            TowerModel raven=gameModel.GetTowerFromId("DartMonkey").Clone<TowerModel>();
            raven.mods=new(0);
            raven.name=Name;
            raven.baseId=raven.name;
            raven.towerSet=TowerSet.Support;
            raven.cost=800;
            raven.tier=0;
            raven.tiers=new[]{0,0,0};
            raven.upgrades=new UpgradePathModel[]{new("Seeker Missile",Name+"-100")};
            raven.range=45;
            raven.radius=8;
            raven.display=new(Name+"-Prefab");
            raven.icon=new("Ui["+Name+"-Icon]");
            raven.instaIcon=raven.icon;
            raven.portrait=new("Ui["+Name+"-Portrait]");
            List<Model>ravenBehav=raven.behaviors.ToList();
            DisplayModel display=ravenBehav.GetModel<DisplayModel>();
            display.display=raven.display;
            display.positionOffset=new(0,0,190);
            ravenBehav.RemoveModel<AttackModel>();
            ravenBehav.Add(new RangeSupportModel("RangeSupportModel",true,0.15f,0,BuffId,
                new(new[]{new FilterInBaseTowerIdModel("TowerFilter",new(new[]{""}))}),false,BuffId,BuffId));
            ravenBehav.Add(new VisibilitySupportModel("VisibilitySupportModel",true,BuffId,false,null,BuffId,BuffId));
            ravenBehav.Add(SelectedSoundModel);
            raven.behaviors=ravenBehav.ToArray();
            SetSounds(raven,Name+"-",true,true,true,false);
            return raven;
        }
        public TowerModel Seeker(){
            TowerModel raven=Base().Clone<TowerModel>();
            raven.name=Name+"-100";
            raven.tier=1;
            raven.tiers=new[]{1,0,0};
            raven.appliedUpgrades=new(new[]{"Seeker Missile"});
            List<Model>ravenBehav=raven.behaviors.ToList();
            AttackModel seeker=gameModel.GetTowerFromId("BombShooter").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
            seeker.name="Seeker";
            seeker.range=raven.range+15;
            seeker.weapons[0].rate=22.5f;
            seeker.weapons[0].projectile.display=new(Name+"-SeekerPrefab");
            seeker.weapons[0].projectile.behaviors.GetModel<CreateProjectileOnContactModel>().projectile.behaviors.GetModel<DamageModel>().damage=5;
            seeker.weapons[0].projectile.behaviors.GetModel<CreateProjectileOnContactModel>().projectile.radius=16;
            ravenBehav.Add(seeker);
            raven.behaviors=ravenBehav.ToArray();
            raven.upgrades=new UpgradePathModel[]{new("Theia",Name+"-200")};
            return raven;
        }
        public TowerModel Theia(){
            TowerModel raven=Seeker().Clone<TowerModel>();
            raven.name=Name+"-200";
            raven.tier=2;
            raven.tiers=new[]{2,0,0};
            raven.range+=10;
            raven.portrait=new("Ui["+Name+"-TheiaPortrait]");
            raven.display=new(Name+"-TheiaPrefab");
            raven.behaviors.GetModel<DisplayModel>().display=raven.display;
            raven.appliedUpgrades=new(new[]{"Seeker Missile","Theia"});
            raven.upgrades=new UpgradePathModel[]{new("Corvid Reactor",Name+"-300")};
            List<Model>ravenBehav=raven.behaviors.ToList();
            ravenBehav.GetModel<AttackModel>().range=raven.range+20;
            AttackModel createTurret=CreateTowerAttackModel.Clone<AttackModel>();
            createTurret.name="CreateTurret";
            List<Model>createTurretBehav=createTurret.behaviors.ToList();
            createTurretBehav.Add(new RotateToTargetModel("RotateToTargetModel",false,false,true,0,true,false));
            RandomPositionModel randomPos=createTurretBehav.GetModel<RandomPositionModel>();
            randomPos.minDistance=raven.range-10;
            randomPos.maxDistance=raven.range;
            createTurret.behaviors=createTurretBehav.ToArray();
            WeaponModel weapon=createTurret.weapons[0];
            weapon.rate=15;
            ProjectileModel proj=weapon.projectile;
            proj.display=new(Name+"-TurretProjectilePrefab");
            Il2CppReferenceArray<Model>projBehav=proj.behaviors;
            ArriveAtTargetModel arriveTarget=projBehav.GetModel<ArriveAtTargetModel>();
            arriveTarget.expireOnArrival=true;
            arriveTarget.altSpeed=80;
            DisplayModel display=projBehav.GetModel<DisplayModel>();
            display.display=proj.display;
            display.delayedReveal=0;
            display.positionOffset=new(0,0,0);
            projBehav.GetModel<CreateTowerModel>().tower=gameModel.GetTowerFromId("AutoTurret");
            ravenBehav.Add(createTurret);
            raven.behaviors=ravenBehav.ToArray();
            return raven;
        }
        public TowerModel Corvid(){
            TowerModel raven=Theia().Clone<TowerModel>();
            raven.name=Name+"-300";
            raven.tier=3;
            raven.tiers=new[]{3,0,0};
            raven.appliedUpgrades=new(new[]{"Seeker Missile","Theia","Corvid Reactor"});
            raven.upgrades=new UpgradePathModel[]{new("Science Vessel",Name+"-400")};
            Il2CppReferenceArray<Model>ravenBehav=raven.behaviors;
            ravenBehav.GetModel<AttackModel>("AttackModel_Seeker").weapons[0].rate=12.5f;
            ravenBehav.GetModel<AttackModel>("AttackModel_CreateTurret").weapons[0].rate=8;
            CreateSoundOnUpgradeModel csoum=ravenBehav.GetModel<CreateSoundOnUpgradeModel>();
            csoum.sound=new(Name+"-ScienceVesselBirth",new(Name+"-ScienceVesselBirth"));
            csoum.sound1=csoum.sound;
            csoum.sound2=csoum.sound;
            csoum.sound3=csoum.sound;
            csoum.sound4=csoum.sound;
            csoum.sound5=csoum.sound;
            csoum.sound6=csoum.sound;
            csoum.sound7=csoum.sound;
            csoum.sound8=csoum.sound;
            return raven;
        }
        public TowerModel Science(){
            TowerModel raven=Corvid().Clone<TowerModel>();
            raven.name=Name+"-400";
            raven.tier=4;
            raven.tiers=new[]{4,0,0};
            raven.portrait=new("Ui["+Name+"-ScienceVesselPortrait]");
            raven.appliedUpgrades=new(new[]{"Seeker Missile","Theia","Corvid Reactor","Science Vessel"});
            raven.upgrades=new UpgradePathModel[0];
            raven.display=new(Name+"-ScienceVesselPrefab");
            List<Model>ravenBehav=raven.behaviors.ToList();
            DisplayModel display=ravenBehav.GetModel<DisplayModel>();
            display.ignoreRotation=true;
            display.display=raven.display;
            ravenBehav.Remove(ravenBehav.First(a=>a.name=="AttackModel_Seeker"));
            AttackModel irradiate=gameModel.GetTowerFromId("GlueGunner-200").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
            irradiate.name="Irradiate";
            irradiate.range=raven.range;
            WeaponModel weapon=irradiate.weapons[0];
            weapon.rate=0.45f;
            ProjectileModel proj=weapon.projectile;
            proj.display=new("");
            List<Model>projBehav=proj.behaviors.ToList();
            projBehav.RemoveModel<CreateSoundOnProjectileCollisionModel>();
            projBehav.GetModel<SlowModifierForTagModel>().slowId="Raven-Gas";
            AddBehaviorToBloonModel addBehav=projBehav.GetModel<AddBehaviorToBloonModel>();
            addBehav.lifespan=15;
            addBehav.overlayType="Raven-Gas";
            DamageOverTimeModel dotModel=addBehav.behaviors.First(a=>a.GetIl2CppType().Name=="DamageOverTimeModel").Cast<DamageOverTimeModel>();
            dotModel.Interval=1.1f;
            dotModel.damage=10;
            dotModel.damageModifierModels.AddItem(new DamageModifierForTagModel("DamageModifierForTagModel","Moab",0.5f,0,false,true));
            SlowModel slow=projBehav.GetModel<SlowModel>();
            slow.multiplier=1;
            slow.overlayType="Raven-Gas";
            slow.overlayLayer=1;
            slow.mutationId="Raven-Gas";
            slow.Mutator.multiplier=slow.multiplier;
            slow.Mutator.overlayType=slow.overlayType;
            slow.Mutator.id="Raven-Gas";
            proj.behaviors=projBehav.ToArray();
            ravenBehav.Add(irradiate);
            raven.behaviors=ravenBehav.ToArray();
            SetSounds(raven,Name+"-ScienceVessel",true,true,true,false);
            return raven;
        }
        public override void Attack(Weapon weapon){
            PlayAnimation(weapon.attack.tower.Node.graphic.GetComponent<Animator>(),"Attack");
        }
    }
}