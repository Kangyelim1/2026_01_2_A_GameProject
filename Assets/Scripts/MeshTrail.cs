using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f; 
    public MovementInput moveScript;
    public float speedBoost = 6;
    public Animator animator;
    public float animSpeedBoost = 1.5f;

    [Header("Mesh Releted")]
    public float meshRefreshRate = 1.0f;
    public float meshDestroyDelay = 3.0f;
    public Transform positionToSpawn;

    [Header("Shader Releted")]
    public Material mat;
    public string ShaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    private SkinnedMeshRenderer[] skinnedRenderer;
    private bool isTrailActive;

    private float normalSpeed;
    private float normalAnimSpeed;

    IEnumerator AnimateMaterialFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(ShaderVarRef);        //알파 값을 가져온다.

        //목표 값에 도달 할 때 까지 반복
        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(ShaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)
    {
        //이전 내용 변수들 저장
        normalSpeed = moveScript.movementSpeed;            //현재 속도를 저장하고 증가된 속도 적용
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");  //현재 애니메이션 속도 저장하고 증가된 속도 적용
        animator.SetFloat("animSpeed", animSpeedBoost);

        while (timeActivated > 0)
        {
            if (skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();    //생성된 위치의 랜더러 컴포넌트들을 가져옴

            for (int i = 0; i < skinnedRenderer.Length; i++)        //각 메시 렌더러에 대한 잔상 생성
            {
                GameObject gObj = new GameObject();                //새로운 오브젝트 생성
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();                               //현재 캐릭터의 포즈를 메시로 변환
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;

                //잔상의 페이드 아웃 효과 시작
                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);                   //일정 시간 후 잔상 제거
            }

            //다음 잔상 생성 까지 대기
            yield return new WaitForSeconds(meshRefreshRate);
        }

        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;


    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)    //스페이스바를 누르고 현재 잔상 효과가 비 활성화 일 때
        {
            isTrailActive = true;                               //잔상 효과 시작 설정
            StartCoroutine(ActivateTrail(activeTime));          //잔상 효과 코루틴 시작
        }
    }
}
