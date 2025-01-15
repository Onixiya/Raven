namespace Raven{
    public class AutoTurret:SC2Tower{
        public override string Name=>"AutoTurret";
        public override Faction TowerFaction=>Faction.Terran;
        public override bool Upgradable=>false;
        public override bool AddToShop=>false;
        public override bool HasBundle=>false;
        [RegisterTypeInIl2Cpp]
        public class AutoTurretCom:MonoBehaviour{
            public AutoTurretCom(IntPtr ptr):base(ptr){}
            void Start(){
                Anim=GetComponent<Animator>();
            }
            Animator Anim;
			int counter=0;
            void Update(){
                if(Anim!=null&&!Anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
					counter++;
					if(counter>360){
						counter=1;
					}
                    transform.Rotate(new(0,counter,0),Space.Self);
				}
			}
        }
        public override TowerModel[]GenerateTowerModels(){
            return new TowerModel[]{
                Base()
            };
        }
        public TowerModel Base(){
            TowerModel turret=gameModel.GetTowerFromId("Sentry").Clone<TowerModel>();
            turret.name=Name;
            turret.baseId=turret.name;
            turret.display=new("SC2Raven-TurretBasePrefab");
            turret.portrait=new("SC2Raven-TurretPortrait");
            Il2CppReferenceArray<Model>turretBehav=turret.behaviors;
            turretBehav.GetModel<TowerExpireModel>().lifespan=20;
            turretBehav.GetModel<DisplayModel>().display=turret.display;
            AttackModel attack=turretBehav.GetModel<AttackModel>();
            Il2CppReferenceArray<Model>attackBehav=attack.behaviors;
            attackBehav.GetModel<RotateToTargetModel>().onlyRotateDuringThrow=false;
            attackBehav.GetModel<DisplayModel>().display=new("SC2Raven-TurretGunPrefab");
            WeaponModel weapon=attack.weapons[0];
            weapon.rate=0.4f;
            List<WeaponBehaviorModel>weaponBehav=new();
            weaponBehav.Add(gameModel.GetTowerFromId("SniperMonkey").behaviors.GetModel<AttackModel>().weapons[0].behaviors.GetModelClone<EjectEffectModel>());
            weapon.emission=new InstantDamageEmissionModel("InstantDamageEmissionModel",new(0));
            weapon.behaviors=weaponBehav.ToArray();
            ProjectileModel proj=weapon.projectile;
            proj.display=new("");
            List<Model>projBehav=proj.behaviors.ToList();
            projBehav.GetModel<DamageModel>().damage=0.7f;
            projBehav.Add(new InstantModel("InstantModel",false,false,true));
            projBehav.RemoveModel<TravelStraitModel>();
            proj.behaviors=projBehav.ToArray();
            return turret;
        }
        public override void Attack(Weapon weapon){
            PlayAnimation(weapon.attack.entity.displayBehaviorCache.node.graphic.GetComponent<Animator>(),"Attack");
        }
    }
}