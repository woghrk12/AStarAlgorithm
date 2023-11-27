using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    private Transform characterTransform = null;
    private Animator characterAnimator = null;
    
    private Transform spriteTransform = null;

    [SerializeField] private float moveSpeed = 0f;

    private int moveHash = 0;

    private void Awake()
    {
        characterTransform = GetComponent<Transform>();
        characterAnimator = GetComponent<Animator>();

        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;

        moveHash = Animator.StringToHash(AnimatorKey.Character.IsMove);
    }

    private void Move(Vector3 moveDir)
    {
        characterTransform.position += moveDir * Time.fixedDeltaTime * moveSpeed;

        if (moveDir.x != 0f)
        {
            spriteTransform.localScale = new Vector3(moveDir.x < 0f ? -0.5f : 0.5f, 0.5f, 1f);
        }

        characterAnimator?.SetBool(moveHash, moveDir != Vector3.zero);
    }
}
