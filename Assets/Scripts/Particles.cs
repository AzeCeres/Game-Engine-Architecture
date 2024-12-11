using UnityEngine;
struct DODParticles
{
    public readonly GameObject[] Particles;
    // needs meshFilter w/ mesh, meshRenderer w/ material.
    // Contains transform by default, position and transforms(scale,rotation etc) is *NOT* DOD structured, but it would most likely be loads slower to do calculations on a separate vec3 array,
    // and then apply it to the same transform later. As we would have to traverse and find the positions there anyway. 
    // also comes with the unfortunate downside of expensive position calculations in relation to parent child chains. This is somewhat mitigated by only having 1 depth to the chain
    // and having the parent aka this. being at 0,0,0
    // theres a lot of optimization to be done, such as GPU instancing since all of the particles are the same etc. But that's outside of the scope of the folder.
    public readonly Material[] Materials;  //reference type, contains colors and such, unfortunately not DOD structured. 
    public readonly Vector3 [] Velocities;
    public readonly Vector2 [] Lifetime; // .x is the timer, ticking down to 0, whilst y is the max or original. to calculate how far along it's journey it is.
    public readonly bool [] IsLive;
    public readonly bool [] Respawns;
    public readonly int Size;
    public DODParticles(int sizeIn)
    {
        Size      = sizeIn;
        Particles  = new GameObject[Size];
        Materials  = new Material  [Size];
        Velocities = new Vector3   [Size];
        Lifetime   = new Vector2   [Size];
        IsLive     = new bool      [Size];
        Respawns   = new bool      [Size];
    }
}
public class Particles : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField][Range(0,5000)] private int   numberOfParticles;
    [SerializeField][Range(0, 100)] private float gravity;
    [Header("Visuals")]
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Mesh     particleMesh;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField][Tooltip("minScale-maxScale")] private Vector2 startScale;
    [Header("Timer")]
    [SerializeField][Tooltip("minTime-maxTime")] private Vector2 timeToLive;
    [SerializeField] private float endScale;
    [SerializeField] private bool isRespawning = true;
    [Header("Position")]
    [SerializeField][Tooltip("minX-maxX")] private Vector2 spawnBoundsX;
    [SerializeField][Tooltip("minY-maxY")] private Vector2 spawnBoundsY;
    [SerializeField][Tooltip("minZ-maxZ")] private Vector2 spawnBoundsZ;

    private DODParticles _particles;
    private Transform _cameraTrans;
    private void Start()
    {
        _cameraTrans = FindFirstObjectByType<Camera>().transform; // would have to look at tags if there are multiple cameras, but since it's a simple scene further checks are unnecessary. 
        _particles = new DODParticles(numberOfParticles);
        particleMaterial.color = startColor;
        for (var i = 0; i < numberOfParticles; i++)
        {
            ConstructParticle(i);
            FillParticle(i); // might want to separate this one out to update, to fill them in little by little 
        }
    }
    private void ConstructParticle(int i)
    {
        var tempMesh = particleMesh;
        _particles.Materials[i] = new Material(particleMaterial);
        _particles.Particles[i] = new GameObject
        {
            name = "particleObject" + i.ToString(),
            transform = { parent = this.transform }
        };
        _particles.Particles[i].AddComponent<MeshFilter>();
        var tempMeshFilter = _particles.Particles[i].GetComponent<MeshFilter>();
        tempMeshFilter.mesh = tempMesh;
        _particles.Particles[i].AddComponent<MeshRenderer>();
        var tempMeshRenderer = _particles.Particles[i].GetComponent<MeshRenderer>();
        tempMeshRenderer.materials[0] = _particles.Materials[i];
    }
    private void FillParticle(int i)
    {
        var scale= Random.Range(startScale.x,   startScale.y);
        var xPos = Random.Range(spawnBoundsX.x, spawnBoundsX.y);
        var yPos = Random.Range(spawnBoundsY.x, spawnBoundsY.y);
        var zPos = Random.Range(spawnBoundsZ.x, spawnBoundsZ.y);
        var xVel = Mathf.Sin(i) / 2;
        var yVel = -(gravity + Random.Range(0f, gravity / 4f));
        var zVel = Mathf.Cos(i) / 2;
        _particles.Velocities[i]   = new Vector3(xVel, yVel, zVel);
        _particles.Particles [i].transform.localScale = new Vector3(scale, scale, scale);
        _particles.Particles [i].transform.position = new Vector3(xPos, yPos, zPos);
        _particles.Lifetime  [i].x = Random.Range(timeToLive.x, timeToLive.y);
        _particles.Lifetime  [i].y = _particles.Lifetime[i].x;
        _particles.IsLive    [i]   = true;
        _particles.Respawns  [i]   = isRespawning;
        _particles.Materials [i].color = startColor;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateParticlesLifetime(_particles);
        UpdateParticlesPosition(_particles);
        UpdateParticlesVisuals (_particles);
    }
    private void UpdateParticlesLifetime(DODParticles particlesIn)
    {
        for (int i = 0; i < particlesIn.Size; i++)
        {
            if (!particlesIn.IsLive[i]) continue;
            particlesIn.Lifetime[i].x -= Time.deltaTime;
            if (particlesIn.Lifetime[i].x > 0) continue;
            if (particlesIn.Respawns[i]) 
            {
                FillParticle(i);
            }
            else
            {
                //particlesIn.particles[i].SetActive(false);
                Destroy(particlesIn.Particles[i]);
                _particles.IsLive[i] = false;
            }
        }
        
    }
    private void UpdateParticlesPosition(DODParticles particlesIn)
    {
        for (int i = 0; i < particlesIn.Size; i++)
        {
            if (!particlesIn.IsLive[i]) continue;
            particlesIn.Particles[i].transform.position += particlesIn.Velocities[i]*Time.deltaTime;
            particlesIn.Particles[i].transform.LookAt(_cameraTrans);
            particlesIn.Particles[i].transform.rotation = Quaternion.LookRotation(_cameraTrans.forward);
        }
    }
    private void UpdateParticlesVisuals(DODParticles particlesIn)
    {
        for (int i = 0; i < particlesIn.Size; i++)
        {
            if (!particlesIn.IsLive[i]) continue;
            var progress = (particlesIn.Lifetime[i].y - particlesIn.Lifetime[i].x)/particlesIn.Lifetime[i].y; // 0 when it has just started, 1 when finished
            var curScale = particlesIn.Particles[i].transform.localScale.x; // assumes same scale transforms on all axis.
            var newScale = Mathf.Lerp(curScale, endScale, progress);
            particlesIn.Particles[i].transform.localScale = new Vector3(newScale, newScale, newScale);
            var curColor = particlesIn.Materials[i].color;
            particlesIn.Materials[i].color = new Color(Mathf.Lerp(curColor.r, endColor.r, progress), Mathf.Lerp(curColor.g, endColor.g, progress),
                Mathf.Lerp(curColor.b, endColor.b, progress), Mathf.Lerp(curColor.a, endColor.a, progress)); // RGBA
        }
    }
}
