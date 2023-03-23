using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;

namespace Raven{
    public class AutoTurret:SC2Tower{
        public override string Name=>"AutoTurret";
        public override Faction TowerFaction=>Faction.Terran;
        public override bool Upgradable=>false;
        public override bool AddToShop=>false;
		public override Dictionary<string,Il2CppSystem.Type>Components=>new(){{"AutoTurret-GunPrefab",Il2CppType.Of<AutoTurretCom>()}};
        [RegisterTypeInIl2Cpp]
        public class AutoTurretCom:MonoBehaviour{
            public AutoTurretCom(IntPtr ptr):base(ptr){}
			int counter=0;
            void Update(){
                if(!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("AutoTurret-Attack")){
					counter++;
					if(counter>360){
						counter=1;
					}
					transform.Rotate(new(0,transform.rotation.y-counter,0),Space.Self);
				}
			}
        }
        public override TowerModel[]GenerateTowerModels(){
            return new TowerModel[]{
                Base()
            };
        }
        public static TowerModel Base(){
            TowerModel turret=Game.instance.model.GetTowerFromId("Sentry").Clone<TowerModel>();
            turret.baseId="AutoTurret";
            turret.name=turret.baseId;
            turret.display=new(){guidRef="AutoTurret-BasePrefab"};
            turret.portrait=new(){guidRef="AutoTurret-Portrait"};
            turret.behaviors.GetModel<TowerExpireModel>().lifespan=20;
            turret.behaviors.GetModel<DisplayModel>().display=turret.display;
            AttackModel attack=turret.behaviors.GetModel<AttackModel>();
            Il2CppReferenceArray<Model>attackBehav=attack.behaviors;
            attackBehav.GetModel<RotateToTargetModel>().onlyRotateDuringThrow=false;
            attackBehav.GetModel<DisplayModel>().display=new(){guidRef="AutoTurret-GunPrefab"};
            WeaponModel weapon=attack.weapons[0];
            weapon.rate=0.5f;
            List<WeaponBehaviorModel>weaponBehav=new();
            weaponBehav.Add(Game.instance.model.GetTowerFromId("SniperMonkey").behaviors.GetModel<AttackModel>().weapons[0].behaviors.First().Clone<EjectEffectModel>());
            weapon.emission=new InstantDamageEmissionModel("InstantDamageEmissionModel",new(0));
            weapon.behaviors=weaponBehav.ToArray();
            ProjectileModel proj=weapon.projectile;
            proj.display=new(){guidRef=""};
            List<Model>projBehav=proj.behaviors.ToList();
            projBehav.GetModel<DamageModel>().damage=0.6f;
            projBehav.Add(new InstantModel("InstantModel",true));
            projBehav.Remove(projBehav.First(a=>a.GetIl2CppType().Name=="TravelStraitModel"));
            return turret;
        }
        public override void Attack(Weapon weapon){
            PlayAnimation(weapon.attack.entity.displayBehaviorCache.node.graphic.GetComponent<Animator>(),"AutoTurret-Attack");
        }
    }
}