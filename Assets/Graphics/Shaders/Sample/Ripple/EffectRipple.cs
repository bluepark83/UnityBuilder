using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EffectRipple : MonoBehaviour
{
    private ParticleSystem ps;
    public float effectInterval = 0.4f;
    public float effectLifeTime = 1.2f;
    private float footGap = 0.05f;
    private int rightFootdir = 1;
    
    
    private Vector3 lastFootprintPostiion;
    private Vector3 lastWavePosition;
    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        MakeEffect();
    }
    
    private void InitParticle()
    {
        ps.Clear();
    }

    /// <summary>
    /// 발자국에 따라 이펙트를 만드는 코드
    /// </summary>
    private void MakeEffect()
    {
        if (Vector3.Distance(lastFootprintPostiion, transform.position) > effectInterval)
        {
            InitParticle();
            
            // ReSharper disable once Unity.InefficientMultiplicationOrder
            Vector3 pos = transform.position + (transform.right * footGap * rightFootdir);
            rightFootdir *= -1;

            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams
            {
                position = pos,
                startLifetime = effectLifeTime
            };
           
            ep.rotation = transform.rotation.y;
            ps.Emit(ep, 1);

            lastFootprintPostiion = transform.position;
        }
    }
}