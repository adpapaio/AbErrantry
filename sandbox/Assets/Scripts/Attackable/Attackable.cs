using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character2D
{
    public abstract class Attackable : MonoBehaviour
    {
        protected Character character;
        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected Animator anim;

        public event Action<Attackable> OnAttackableDestroyed;

        protected string attackNoise;

        protected int maxVitality;
        protected int currentVitality;

        public bool isDying;

        public bool canFlinch;
        public bool canKnockBack;
        public bool canTakeDamage;

        //used for initialization
        protected void Start()
        {
            isDying = false;
            character = GetComponent<Character>();
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();

            maxVitality = currentVitality = character.fields.vitality;
        }

        //applies damage to the player
        public virtual void TakeDamage(GameObject attacker, int damage, bool appliesKnockback = true)
        {
            if (!isDying)
            {
                if (canFlinch && canTakeDamage)
                {
                    Flinch();
                }

                if (canKnockBack && appliesKnockback)
                {
                    KnockBack(attacker.transform.position, damage);
                }

                if (canTakeDamage)
                {
                    ParticleManager.instance.SpawnParticle(gameObject, "hurt");
                    currentVitality = currentVitality - damage;
                    if (currentVitality <= 0f)
                    {
                        Die();
                    }
                }
            }
        }

        public virtual void Kill()
        {
            currentVitality = 0;
            TakeDamage(gameObject, 0);
        }

        protected void KnockBack(Vector3 attackerLocation, float intensity)
        {
            Vector3 force;
            if (attackerLocation.x < transform.position.x)
            {
                force = new Vector3(intensity * 2.5f, 2.5f, 0.0f);
            }
            else
            {
                force = new Vector3(-(intensity) * 2.5f, 2.5f, 0.0f);
            }
            rb.velocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse);
            if (GetComponent<CharacterMovement>() != null)
            {
                GetComponent<CharacterMovement>().vLast = rb.velocity.x;
            }
        }

        protected virtual void Flinch()
        {
            anim.Play("FLINCH");
        }

        protected void Die()
        {
            InitializeDeath();
            if (OnAttackableDestroyed != null)
            {
                OnAttackableDestroyed(this);
            }
        }

        protected abstract void InitializeDeath();

        public abstract void FinalizeDeath();
    }
}
