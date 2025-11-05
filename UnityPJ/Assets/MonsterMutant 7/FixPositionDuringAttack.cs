using Unity.VisualScripting;
using UnityEngine;

public class FixPositionDuringAttack : MonoBehaviour
{
    Vector3 fixedPosition;
    bool isAttacking = false;

    private Animator anim;
    public int totalFrames = 60;
    public int startFrame = 15;
    private void Start()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
            return;
        }

        float startNormalizedTime = (float)startFrame / totalFrames;
        anim.Play("rage(Read_Only)", 0, startNormalizedTime);
    }
    void Update()
    {
        if (isAttacking)
            transform.position = fixedPosition;
    }

    public void StartAttack()
    {
        isAttacking = true;
        fixedPosition = transform.position;  // 공격 시작 시 현재 위치 저장
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}
