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
        public override Faction TowerFaction=>Faction.Terran;
        public override int MaxTier=>4;
        public override UpgradeModel[]GenerateUpgradeModels(){
            return new UpgradeModel[]{
                new("Seeker Missile",900,0,new(){guidRef="Ui[Raven-SeekerMissileIcon]"},0,0,0,"","Seeker Missile"),
                new("Theia",2025,0,new(){guidRef="Ui[Raven-TheiaIcon]"},0,1,0,"","Theia"),
                new("Corvid Reactor",4150,0,new(){guidRef="Ui[Raven-CorvidReactorIcon]"},0,2,0,"","Corvid Reactor"),
                new("Science Vessel",11140,0,new(){guidRef="Ui[Raven-ScienceVesselIcon]"},0,3,0,"","Science Vessel")
            };
        }
        public override Dictionary<string,Il2CppSystem.Type>Components=>new(){{"Raven-Prefab",Il2CppType.Of<RavenCom>()}};
        [RegisterTypeInIl2Cpp]
        public class RavenCom:MonoBehaviour{
            public RavenCom(IntPtr ptr):base(ptr){}
			public GameObject activeObj=null;
			public GameObject raven=null;
			public GameObject theia=null;
			public GameObject sciVessel=null;
            float timer=0;
			int selectSound=0;
			int upgradeSound=0;
			void Start(){
				raven=transform.GetChild(0).gameObject;
				theia=transform.GetChild(1).gameObject;
				sciVessel=transform.GetChild(2).gameObject;
				theia.SetActive(false);
				sciVessel.SetActive(false);
				raven.transform.localPosition=new(0,0,0);
				theia.transform.localPosition=new(0,0,0);
				sciVessel.transform.localPosition=new(0,0,0);
				activeObj=raven;
			}
            void Update(){
                timer+=Time.fixedDeltaTime;
                if(timer>10){
                    if(activeObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Raven-Stand")){
                        switch(new System.Random().Next(1,101)){
                            case<16:
                                if(gameObject.name.Contains("Theia")){
                                    PlayAnimation(activeObj.GetComponent<Animator>(),"Raven-Fidget"+new System.Random().Next(1,3),0.4f);
                                }else{
                                    PlayAnimation(activeObj.GetComponent<Animator>(),"Raven-Fidget"+new System.Random().Next(1,4),0.4f);
                                }
                                break;
                        }
                    }
                    timer=0;
                }
            }
			public void PlaySelectSound(){
				if(selectSound>5){
					selectSound=0;
				}
				selectSound+=1;
				if(activeObj.name.Contains("Science")){
					PlaySound("Raven-ScienceVesselSelect"+selectSound);
				}else{
					Log(selectSound);
					PlaySound("Raven-Select"+selectSound);
				}
			}
			public void PlayUpgradeSound(){
				upgradeSound+=1;
				selectSound=0;
				PlaySound("Raven-Upgrade"+upgradeSound);
			}
        }
        [HarmonyPatch(typeof(FilterInBaseTowerId),"FilterTowerModel")]
        public class FilterInBaseTowerIdFilterTower_Patch{
            [HarmonyPrefix]
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
        public static string BuffId="Ui[Raven-RangeBuffIcon]";
        public override ShopTowerDetailsModel ShopDetails(){
            ShopTowerDetailsModel details=gameModel.towerSet[0].Clone<ShopTowerDetailsModel>();
            details.towerId=Name;
            details.name=Name;
            details.towerIndex=14;
            details.pathOneMax=4;
            details.pathTwoMax=0;
            details.pathThreeMax=0;
            details.popsRequired=0;
            List<BuffIndicatorModel>buffs=gameModel.buffIndicatorModels.ToList();
            BuffIndicatorModel buff=new BuffIndicatorModel("RavenRangeBuff",BuffId,BuffId,false,0,false);
            buffs.Add(buff);
            gameModel.buffIndicatorModels=buffs.ToArray();
            GameData.Instance.buffIconSprites.buffIconSprites.Add(new(){buffId=BuffId,icon=new(){guidRef=BuffId}});
			LocManager.textTable.Add("Raven Description","Terran flying support. Buffs range and gives camo detection to nearby towers");
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
            raven.name=Name;
            raven.baseId=raven.name;
            raven.towerSet=TowerSet.Support;
            raven.cost=800;
            raven.tier=0;
            raven.tiers=new[]{0,0,0};
            raven.upgrades=new UpgradePathModel[]{new("Seeker Missile",Name+"-100")};
            raven.range=45;
            raven.radius=8;
            raven.display=new(){guidRef="Raven-Prefab"};
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
            ravenBehav.Add(new VisibilitySupportModel("VisibilitySupportModel",true,BuffId,false,null,BuffId,BuffId));
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
            AttackModel seeker=gameModel.GetTowerFromId("BombShooter").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
            seeker.name="Seeker";
            seeker.range=raven.range+15;
            seeker.weapons[0].rate=22.5f;
            seeker.weapons[0].projectile.display=new(){guidRef="Raven-SeekerPrefab"};
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
            proj.display=new(){guidRef="AutoTurret-ProjectilePrefab"};
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
            raven.portrait=new(){guidRef="Ui[Raven-ScienceVesselPortrait]"};
            raven.appliedUpgrades=new(new[]{"Seeker Missile","Theia","Corvid Reactor","Science Vessel"});
            raven.upgrades=new UpgradePathModel[0];
            List<Model>ravenBehav=raven.behaviors.ToList();
            ravenBehav.GetModel<DisplayModel>().ignoreRotation=true;
            ravenBehav.Remove(ravenBehav.First(a=>a.name=="AttackModel_Seeker"));
            AttackModel irradiate=gameModel.GetTowerFromId("GlueGunner-200").behaviors.GetModel<AttackModel>().Clone<AttackModel>();
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
        public override void Create(Tower tower){
            PlaySound("Raven-Birth");
        }
        public override void Upgrade(int tier,Tower tower){
			RavenCom com=tower.Node.graphic.gameObject.GetComponent<RavenCom>();
            switch(tier){
				case 2:
					com.activeObj.SetActive(false);
					com.activeObj=com.theia;
					com.activeObj.SetActive(true);
					tower.Node.graphic.gameObject.GetComponent<RavenCom>().PlayUpgradeSound();
					break;
                case 4:
					com.activeObj.SetActive(false);
					com.activeObj=com.sciVessel;
					com.activeObj.SetActive(true);
                    PlaySound("Raven-ScienceVesselBirth");
                    break;
                default:
                    tower.Node.graphic.gameObject.GetComponent<RavenCom>().PlayUpgradeSound();
                    break;
            }
        }
        public override void Select(Tower tower){
            tower.Node.graphic.gameObject.GetComponent<RavenCom>().PlaySelectSound();
        }
        public override void Attack(Weapon weapon){
            PlayAnimation(weapon.attack.tower.Node.graphic.GetComponent<RavenCom>().activeObj.GetComponent<Animator>(),"Raven-Attack");
        }
        public override bool Ability(string ability,Tower tower){
            PlayAnimation(tower.Node.graphic.GetComponent<RavenCom>().activeObj.GetComponent<Animator>(),"Raven-Attack");
			return true;
        }
    }
}