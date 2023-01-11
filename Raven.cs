[assembly:MelonGame("Ninja Kiwi","BloonsTD6")]
[assembly:MelonInfo(typeof(Raven.ModMain),Raven.ModHelperData.Name,Raven.ModHelperData.Version,"Silentstorm")]
[assembly:MelonOptionalDependencies("SC2ExpansionLoader")]
namespace Raven{
    public class ModMain:MelonMod{
        public static string LoaderPath=MelonEnvironment.ModsDirectory+"/SC2ExpansionLoader.dll";
        public override void OnEarlyInitializeMelon(){
            if(!File.Exists(LoaderPath)){
                var stream=new HttpClient().GetStreamAsync("https://github.com/Onixiya/SC2Expansion/releases/latest/download/SC2ExpansionLoader.dll");
                var fileStream=new FileStream(LoaderPath,FileMode.CreateNew);
                stream.Result.CopyToAsync(fileStream);
                Log("Exiting game so MelonLoader correctly loads all mods");
                Application.Quit();
            }
        }
    }
    public class Raven:SC2Tower{
        public override string Name=>"Raven";
        public override UpgradeModel[]Upgrades(){
            return new UpgradeModel[]{
                new("Seeker Missile",900,0,new(){guidRef="Ui[Raven-SeekerMissileIcon]"},0,0,0,"","Seeker Missile"),
                new("Theia",2025,0,new(){guidRef="Ui[Raven-TheiaIcon]"},0,1,0,"","Theia"),
                new("Corvid Reactor",4150,0,new(){guidRef="Ui[Raven-CorvidReactorIcon]"},0,2,0,"","Corvid Reactor"),
                new("Science Vessel",11140,0,new(){guidRef="Ui[Raven-ScienceVesselIcon]"},0,3,0,"","Science Vessel")
            };
        }
        public override int MaxSelectQuote=>7;
        public override int MaxUpgradeQuote=>4;
        public override Dictionary<string,string>SoundNames=>new(){{"Raven","Raven-"},{"Theia","Raven-"},{"ScienceVessel","Raven-ScienceVessel"}};
        public override Dictionary<string,Il2CppSystem.Type>Behaviours=>new(){{"Raven-Raven-Prefab",Il2CppType.Of<RavenBehavior>()},
            {"Raven-Theia-Prefab",Il2CppType.Of<RavenBehavior>()}};
        [RegisterTypeInIl2Cpp]
        public class RavenBehavior:MonoBehaviour{
            public RavenBehavior(IntPtr ptr):base(ptr){}
            float Timer=0;
            public void Update(){
                Timer+=Time.fixedDeltaTime;
                if(Timer>10){
                    if(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Raven-Stand")){
                        switch(new System.Random().Next(1,101)){
                            case<16:
                                if(gameObject.name.Contains("Theia")){
                                    PlayAnimation(GetComponent<UnityDisplayNode>(),"Raven-Fidget"+new System.Random().Next(1,3),0.4f);
                                }else{
                                    PlayAnimation(GetComponent<UnityDisplayNode>(),"Raven-Fidget"+new System.Random().Next(1,4),0.4f);
                                }
                                break;
                        }
                    }
                    Timer=0;
                }
            }
        }
        [HarmonyPatch(typeof(FilterInBaseTowerId),"FilterTowerModel")]
        public class FilterInBaseTowerIdFilterTower_Patch{
            [HarmonyPrefix]
            //__instance.entity.blahblahblah is the tower with the filter, towerModel is the tower thats being filtered out or in
            public static bool Prefix(FilterInBaseTowerId __instance,ref bool __result,TowerModel towerModel){
                if(__instance.entity.dependants[0].Cast<RangeSupport>().tower.towerModel.baseId!="Raven"){
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
        public static string BuffId="Ui[Raven-RangeBuffIcon]";
        public override ShopTowerDetailsModel ShopDetails(){
            ShopTowerDetailsModel details=Game.instance.model.towerSet[0].Clone<ShopTowerDetailsModel>();
            details.towerId=Name;
            details.name=Name;
            details.towerIndex=14;
            details.pathOneMax=4;
            details.pathTwoMax=0;
            details.pathThreeMax=0;
            details.popsRequired=0;
            List<BuffIndicatorModel>buffs=Game.instance.model.buffIndicatorModels.ToList();
            BuffIndicatorModel buff=new BuffIndicatorModel("RavenRangeBuff",BuffId,BuffId,false,0,false);
            buffs.Add(buff);
            Game.instance.model.buffIndicatorModels=buffs.ToArray();
            GameData.Instance.buffIconSprites.buffIconSprites.Add(new(){buffId=BuffId,icon=new(){guidRef=BuffId}});
            LocalizationManager.Instance.textTable.Add("Seeker Missile Description","Deploys fast and high damage missiles with target tracking");
            LocalizationManager.Instance.textTable.Add("Theia Description","Advanced Dominion Raven. Places down automatic turrets nearby and increases range");
            LocalizationManager.Instance.textTable.Add("Corvid Reactor Description","Upgraded Raven reactor, allows for much quicker Seeker Missile and Auto Turret production");
            LocalizationManager.Instance.textTable.Add("Science Vessel Description","Manned science craft and predecessor to the Raven. Replaces Seeker Missile with Irradiate, dealing high damage to non Moabs");
            return details;
        }
        public override TowerModel[]TowerModels(){
            return new TowerModel[]{
                Base(),
                Seeker(),
                Theia(),
                Corvid(),
                Science()
            };
        }
        public TowerModel Base(){
            TowerModel raven=Game.instance.model.GetTowerFromId("DartMonkey").Clone<TowerModel>();
            raven.name=Name;
            raven.baseId=raven.name;
            raven.towerSet=TowerSet.Support;
            raven.cost=800;
            raven.tier=0;
            raven.tiers=new[]{0,0,0};
            raven.upgrades=new UpgradePathModel[]{new("Seeker Missile",Name+"-100")};
            raven.range=45;
            raven.radius=8;
            raven.display=new(){guidRef="Raven-Raven-Prefab"};
            raven.icon=new(){guidRef="Ui[Raven-Icon]"};
            raven.instaIcon=raven.icon;
            raven.portrait=new(){guidRef="Ui[Raven-Portrait]"};
            List<Model>ravenBehav=raven.behaviors.ToList();
            DisplayModel display=ravenBehav.GetModel<DisplayModel>();
            display.display=raven.display;
            display.positionOffset=new(0,0,200);
            ravenBehav.Remove(ravenBehav.First(a=>a.GetIl2CppType().Name=="AttackModel"));
            ravenBehav.Add(new RangeSupportModel("RangeSupportModel",true,0.15f,0,BuffId,
                new(new[]{new FilterInBaseTowerIdModel("TowerFilter",new(new[]{""}))}),false,BuffId,BuffId));
            ravenBehav.Add(new VisibilitySupportModel("VisibilitySupportModel",true,"RavenVisibility",null,"",""));
            raven.behaviors=ravenBehav.ToArray();
            return raven;
        }
        public TowerModel Seeker(){
            TowerModel raven=Base().Clone<TowerModel>();
            raven.name=Name+"-100";
            raven.tier=1;
            raven.tiers=new[]{1,0,0};
            raven.appliedUpgrades=new(new[]{"Seeker Missile"});
            List<Model>ravenBehav=raven.behaviors.ToList();
            AttackModel seeker=Game.instance.model.GetTowerFromId("BombShooter").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
            seeker.name="Seeker";
            seeker.range=raven.range+15;
            seeker.weapons[0].rate=22.5f;
            seeker.weapons[0].projectile.display=new(){guidRef="Raven-SeekerMissile-Prefab"};
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
            raven.display=new(){guidRef="Raven-Theia-Prefab"};
            raven.portrait=new(){guidRef="Ui[Raven-TheiaPortrait]"};
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
            proj.display=new(){guidRef="AutoTurret-Projectile-Prefab"};
            Il2CppReferenceArray<Model>projBehav=proj.behaviors;
            ArriveAtTargetModel arriveTarget=projBehav.GetModel<ArriveAtTargetModel>();
            arriveTarget.expireOnArrival=true;
            arriveTarget.altSpeed=80;
            DisplayModel display=projBehav.GetModel<DisplayModel>();
            display.display=proj.display;
            display.delayedReveal=0;
            display.positionOffset=new(0,0,0);
            projBehav.GetModel<CreateTowerModel>().tower=AutoTurret.Base();
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
            return raven;
        }
        public TowerModel Science(){
            TowerModel raven=Corvid().Clone<TowerModel>();
            raven.name=Name+"-400";
            raven.tier=4;
            raven.tiers=new[]{4,0,0};
            raven.display=new(){guidRef="Raven-ScienceVessel-Prefab"};
            raven.portrait=new(){guidRef="Ui[Raven-ScienceVesselPortrait]"};
            raven.appliedUpgrades=new(new[]{"Seeker Missile","Theia","Corvid Reactor","Science Vessel"});
            raven.upgrades=new UpgradePathModel[0];
            List<Model>ravenBehav=raven.behaviors.ToList();
            ravenBehav.GetModel<DisplayModel>().ignoreRotation=true;
            ravenBehav.Remove(ravenBehav.First(a=>a.name=="AttackModel_Seeker"));
            AttackModel irradiate=Game.instance.model.GetTowerFromId("GlueGunner-200").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
            irradiate.name="Irradiate";
            irradiate.range=raven.range;
            WeaponModel weapon=irradiate.weapons[0];
            weapon.rate=0.45f;
            ProjectileModel proj=weapon.projectile;
            proj.display=new(){guidRef=""};
            Il2CppReferenceArray<Model>projBehav=proj.behaviors;
            AddBehaviorToBloonModel addBehav=projBehav.GetModel<AddBehaviorToBloonModel>();
            addBehav.lifespan=15;
            DamageOverTimeModel dotModel=addBehav.behaviors.First(a=>a.GetIl2CppType().Name=="DamageOverTimeModel").Cast<DamageOverTimeModel>();
            dotModel.Interval=1.1f;
            dotModel.damage=10;
            dotModel.damageModifierModels.AddItem(new DamageModifierForTagModel("DamageModifierForTagModel","Moab",0.5f,0,false,true));
            SlowModel slow=projBehav.GetModel<SlowModel>();
            slow.multiplier=1;
            slow.overlayType="GlueDissolve";
            slow.Mutator.multiplier=slow.multiplier;
            slow.Mutator.overlayType=slow.overlayType;
            ravenBehav.Add(irradiate);
            raven.behaviors=ravenBehav.ToArray();
            return raven;
        }
        public override void Create(){
            PlaySound("Raven-Birth");
        }
        public override void Upgrade(int tier,Tower tower){
            switch(tier){
                case 3:
                    PlaySound("Raven-Upgrade3");
                    break;
                case 4:
                    PlaySound("Raven-ScienceVesselBirth");
                    break;
                default:
                    tower.Node.graphic.gameObject.GetComponent<SC2Sound>().PlayUpgradeSound();
                    break;
            }
        }
        public override void Select(Tower tower){
            tower.Node.graphic.gameObject.GetComponent<SC2Sound>().PlaySelectSound();
        }
        public override void Attack(Weapon weapon){
            PlayAnimation(weapon.attack.tower.Node.graphic,"Raven-Attack");
        }
        public override void Ability(string ability,Tower tower){
            PlayAnimation(tower.Node.graphic,"Raven-Attack");
        }
    }
}