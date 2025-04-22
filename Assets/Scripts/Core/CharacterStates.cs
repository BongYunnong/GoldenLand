using System.Buffers;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace CharacterStates
{
    public class Idle : State<CharacterBase, ECharacterState>
    {
        public override void Enter(CharacterBase entity)
        {
        }

        public override void Execute(CharacterBase entity)
        {
        }

        public override void Exit(CharacterBase entity)
        {
        }

        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            return true;
        }
    }


    public class Action : State<CharacterBase, ECharacterState>
    {
        public override void Enter(CharacterBase entity)
        {
        }

        public override void Execute(CharacterBase entity)
        {
        }

        public override void Exit(CharacterBase entity)
        {
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            return true;
        }
    }
    

    public class Stagger : State<CharacterBase, ECharacterState>
    {
        private float staggerElapsedTime = 0;
        private float maxStaggerTime = 1;
        
        public void SetStaggerTime(float maxStaggerTime)
        {
            this.maxStaggerTime = maxStaggerTime;
        }
        
        public override void Enter(CharacterBase entity)
        {
            staggerElapsedTime = 0;
            entity.SetVelocity(Vector2.zero);
            entity.SetTargetVelocity(Vector2.zero);
            entity.SetAdditionalVelocity(Vector2.zero);
        }

        public override void Execute(CharacterBase entity)
        {
            staggerElapsedTime += Time.deltaTime;
            if (staggerElapsedTime >= maxStaggerTime)
            {
                entity.TryChangeState(ECharacterState.Idle);
            }
        }

        public override void Exit(CharacterBase entity)
        {
            staggerElapsedTime = 0;
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            if (newState == ECharacterState.Idle ||
                newState == ECharacterState.Action)
            {
                return staggerElapsedTime >= maxStaggerTime;
            }
            return true;
        }
    }
    
    public class Stun : State<CharacterBase, ECharacterState>
    {
        private float stunElapsedTime = 0;
        private float maxStunTime = 1;
        private VisualEffectComponent stunVisualEffect;

        public void SetStunTime(float maxStunTime)
        {
            this.maxStunTime = maxStunTime;
        }
        
        public override void Enter(CharacterBase entity)
        {
            stunElapsedTime = 0;
            entity.SetVelocity(Vector2.zero);
            entity.SetTargetVelocity(Vector2.zero);
            entity.SetAdditionalVelocity(Vector2.zero);
            
            entity.GameplayTagContainer.AddTag(EGameplayTag.Stunned);


            ClearStunEffect();
            CreateStunEffect(entity);
        }

        private void CreateStunEffect(CharacterBase entity)
        {
            VisualEffectComponent visualEffect = VisualEffectController.Instance.PlayVisualEffect("Stun");
            visualEffect.transform.SetParent(entity.transform);
            visualEffect.SetBaseLocalTransform(entity.GetHeight() * Vector2.up, Quaternion.identity);
            visualEffect.SetInnerTransform(Vector3.zero, Quaternion.identity);
            visualEffect.FlipX(false);
            stunVisualEffect = visualEffect;
        }

        private void ClearStunEffect()
        {
            if (stunVisualEffect != null)
            {
                stunVisualEffect.gameObject.SetActive(false);
                stunVisualEffect = null;
            }
        }

        public override void Execute(CharacterBase entity)
        {
            stunElapsedTime += Time.deltaTime;
            if (stunElapsedTime >= maxStunTime)
            {
                entity.TryChangeState(ECharacterState.Idle);
            }
        }

        public override void Exit(CharacterBase entity)
        {
            entity.GameplayTagContainer.RemoveTag(EGameplayTag.Stunned);

            ClearStunEffect();
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            if (newState == ECharacterState.Idle ||
                newState == ECharacterState.Action)
            {
                return stunElapsedTime >= maxStunTime;
            }
            return true;
        }
    }
    
    
    public class Down : State<CharacterBase, ECharacterState>
    {
        private float downElapsedTime = 0;
        private float maxDownTime = 1;
        private VisualEffectComponent visualEffectComponent;
        
        public override void Enter(CharacterBase entity)
        {
            downElapsedTime += Time.deltaTime;
            entity.SetVelocity(Vector2.zero);
            entity.SetTargetVelocity(Vector2.zero);
            entity.SetAdditionalVelocity(Vector2.zero);
            entity.TryPlayAnimBool("Down", true);
            
            entity.GameplayTagContainer.AddTag(EGameplayTag.Downed);
            
            ClearEffect();
            CreateEffect(entity);
        }
        
        private void CreateEffect(CharacterBase entity)
        {
            VisualEffectComponent visualEffect = VisualEffectController.Instance.PlayVisualEffect("SmokeSplash");
            visualEffect.SetBaseTransform(entity.transform.position, Quaternion.identity);
            visualEffect.SetInnerTransform(Vector3.zero, Quaternion.identity);
            visualEffect.FlipX(false);
            visualEffectComponent = visualEffect;
        }

        private void ClearEffect()
        {
            if (visualEffectComponent != null)
            {
                visualEffectComponent.gameObject.SetActive(false);
                visualEffectComponent = null;
            }
        }

        public override void Execute(CharacterBase entity)
        {
            downElapsedTime += Time.deltaTime;
            if (downElapsedTime >= maxDownTime)
            {
                entity.TryChangeState(ECharacterState.Idle);
            }
        }

        public override void Exit(CharacterBase entity)
        {
            downElapsedTime = 0;
            entity.TryPlayAnimBool("Down", false);
            entity.GameplayTagContainer.RemoveTag(EGameplayTag.Downed);
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            if (newState == ECharacterState.Idle ||
                newState == ECharacterState.Action)
            {
                return downElapsedTime >= maxDownTime;
            }
            return true;
        }
    }

    public class WallHit : State<CharacterBase, ECharacterState>
    {
        private float elapsedTime = 0;
        private float minStaggerTime = 1f;
        private bool staggerFinished = false;

        private bool blockingGravity = false;
        
        private VisualEffectComponent visualEffectComponent;

        public override void Enter(CharacterBase entity)
        {
            elapsedTime = 0;
            entity.SetVelocity(Vector2.zero);
            entity.SetTargetVelocity(Vector2.zero);
            entity.SetAdditionalVelocity(Vector2.zero);
            entity.BlockGravity(true, "WallHit");
            blockingGravity = true;
            staggerFinished = false;
            entity.TryPlayAnimBool("WallHit", true);
            
            
            ClearEffect();
            CreateEffect(entity);
        }

        private void CreateEffect(CharacterBase entity)
        {
            VisualEffectComponent visualEffect = VisualEffectController.Instance.PlayVisualEffect("SmokeSplash");
            visualEffect.SetBaseTransform(entity.GetCenterSocketPosition(), Quaternion.Euler(0, - 20 * 0, 90));
            visualEffect.SetInnerTransform(Vector3.zero, Quaternion.identity);
            visualEffect.FlipX(false);
            visualEffectComponent = visualEffect;
        }

        private void ClearEffect()
        {
            if (visualEffectComponent != null)
            {
                visualEffectComponent.gameObject.SetActive(false);
                visualEffectComponent = null;
            }
        }
        public override void Execute(CharacterBase entity)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= minStaggerTime)
            {
                if (staggerFinished == false)
                {
                    staggerFinished = true;
                    entity.BlockGravity(false, "WallHit");
                    blockingGravity = false;
                }
            }

            if (staggerFinished)
            {
                if (entity.TryGetComponent(out Character character))
                {
                    if (character.PlatformerComponent.collisions.below)
                    {
                        entity.TryChangeState(ECharacterState.Down);
                    }
                }
            }
        }

        public override void Exit(CharacterBase entity)
        {
            elapsedTime = 0;
            if (blockingGravity)
            {
                blockingGravity = false;
                entity.BlockGravity(false, "WallHit");
            }
            entity.TryPlayAnimBool("WallHit", false);
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            if (newState == ECharacterState.Idle ||
                newState == ECharacterState.Action)
            {
                return staggerFinished;
            }
            return true;
        }
    }

    public class Airborne : State<CharacterBase, ECharacterState>
    {
        private float elapsedTime = 0;
        private float minAirborneElapsedTime = 2;
        private float maxStaggerTime = 0.1f;
        private Vector2? force;
        private bool forceTriggered = false;
        
        private VisualEffectComponent strikeVisualEffectComponent;
        
        public void SetStaggerTime(float maxStaggerTime)
        {
            this.maxStaggerTime = maxStaggerTime;
        }
        public void SetAirborneForce(Vector2? force)
        {
            this.force = force;
        }
        
        public override void Enter(CharacterBase entity)
        {
            forceTriggered = false;
            elapsedTime = 0;
            entity.SetVelocity(Vector2.zero);
            entity.SetTargetVelocity(Vector2.zero);
            entity.SetAdditionalVelocity(Vector2.zero);
            entity.TryPlayAnimBool("Airborne", true);
            
            ClearStrikeEffect();
        }

        private void CreateStrikeEffect(CharacterBase entity)
        {
            VisualEffectComponent visualEffect = VisualEffectController.Instance.PlayVisualEffect("SmokeStrike");
            Vector2 force = this.force.Value;
            
            visualEffect.SetBaseTransform(entity.GetCenterSocketPosition(), Quaternion.Euler(0,0,Vector2.Angle(Vector2.right, force)));
            visualEffect.SetInnerTransform(Vector3.zero, Quaternion.identity);
            visualEffect.FlipX(false);
            strikeVisualEffectComponent = visualEffect;
        }

        private void ClearStrikeEffect()
        {
            if (strikeVisualEffectComponent != null)
            {
                strikeVisualEffectComponent.gameObject.SetActive(false);
                strikeVisualEffectComponent = null;
            }
        }
            
        public override void Execute(CharacterBase entity)
        {
            if (entity.TryGetComponent(out Character character))
            {
                if (character.PlatformerComponent.collisions.below && entity.GameplayTagContainer.HasTag(EGameplayTag.Airborne))
                {
                    entity.GameplayTagContainer.RemoveTag(EGameplayTag.Airborne);
                }
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= maxStaggerTime)
            {
                if (forceTriggered == false)
                {
                    forceTriggered = true;
                    entity.SetAdditionalVelocity(force.Value);
                    entity.GameplayTagContainer.AddTag(EGameplayTag.Airborne);
                    
                    CreateStrikeEffect(entity);
                }
            }

            
            if (forceTriggered && elapsedTime >= minAirborneElapsedTime)
            {
                entity.TryChangeState(ECharacterState.Idle);
            }
        }

        public override void Exit(CharacterBase entity)
        {
            elapsedTime = 0;
            entity.TryPlayAnimBool("Airborne", false);
            entity.GameplayTagContainer.RemoveTag(EGameplayTag.Airborne);
        }
        
        public override bool CanTransitionTo(CharacterBase entity, ECharacterState newState)
        {
            if (newState == ECharacterState.Idle ||
                newState == ECharacterState.Action)
            {
                bool staggerEnd = entity.Velocity.y <= 0 && elapsedTime >= maxStaggerTime;
                if (entity.TryGetComponent(out Character character))
                {
                    return staggerEnd && character.PlatformerComponent.collisions.below;
                }
                return staggerEnd;
            }
            return true;
        }
    }
}