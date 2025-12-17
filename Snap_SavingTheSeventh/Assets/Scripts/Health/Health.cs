using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    public float currentHealth {get; private set;}
    private Animator anim;
    private bool dead;

    private void Awake(){
        currentHealth = startingHealth;
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float _damage){
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0){
            anim.SetTrigger("3_Damaged");
        } else {
            if (!dead){
                anim.SetTrigger("4_Death");
                GetComponent<King>().enabled = false;
                dead = true;
            }
        }
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.E))
            TakeDamage(0.5f);
    }
}
