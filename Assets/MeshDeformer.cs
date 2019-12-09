using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    Mesh originalMesh;
    Mesh m_deformingMesh;
    MeshCollider meshCollider;
    Vector3[] m_originalVerts;
    Vector3[] m_displacedVerts;
    Vector3[] m_vertexVelocities;
    public float m_springForce = 20f;
    public float m_damping = 5f;
    public bool holdForce = false;
    bool forceIsApplied;
    Vector3 currentForcePoint;
    Vector4[] ogPoints;
    float currentFourceAmount;

    /**
     * For stylization of deformations
     */
    Renderer rend;
    float[] vertex_damage;
    public float damage_threshold = 2.0f;
    MaterialPropertyBlock propertyBlock;

    struct Spring
    {
        float springConstant;
        float length;
        float restLength;
        float dampingForce;
    }

    public Mesh deformedMesh { get { return m_deformingMesh; } }
    public Vector3[] displacedVerts { get { return m_displacedVerts; } }
    public Vector3[] originalVerts { get { return m_originalVerts; } }

    // Start is called before the first frame update
    void Start()
    {
        m_deformingMesh = GetComponent<MeshFilter>().mesh;
        originalMesh = Instantiate(m_deformingMesh);

        originalMesh.RecalculateNormals();
        // Initialize some temporary arrays to pass in the deformed vertecies and normals to the shader
        var tmpVec2 = new Vector2[m_deformingMesh.vertices.Length];
        var tmpVec1 = new Vector2[m_deformingMesh.vertices.Length];

        var tmpNorm2 = new Vector2[m_deformingMesh.normals.Length];
        var tmpNorm1 = new Vector2[m_deformingMesh.normals.Length];
        // Populate the temporary arrays with copies of the mesh's normals and vertecies
        for (int i = 0; i < m_deformingMesh.vertices.Length; i++)
        {
            tmpNorm2[i] = new Vector2(m_deformingMesh.normals[i].x, m_deformingMesh.normals[i].y);
            tmpNorm1[i] = new Vector2(m_deformingMesh.normals[i].z, 0f);
        }
        for (int i = 0; i < m_deformingMesh.normals.Length; i++)
        {
            tmpNorm2[i] = new Vector2(m_deformingMesh.normals[i].x, m_deformingMesh.normals[i].y);
            tmpNorm1[i] = new Vector2(m_deformingMesh.normals[i].z, 0f);
        }
        // Store the temp arrays in the mesh's extra uv coordinates
        m_deformingMesh.uv2 = tmpVec2;
        m_deformingMesh.uv3 = tmpVec1;
        m_deformingMesh.uv4 = tmpNorm2;
        m_deformingMesh.uv5 = tmpNorm1;

        meshCollider = GetComponent<MeshCollider>();
        m_originalVerts = m_deformingMesh.vertices;
        m_displacedVerts = new Vector3[m_originalVerts.Length];
        vertex_damage = new float[m_originalVerts.Length];
        ogPoints = new Vector4[m_originalVerts.Length];
        for (int i = 0; i < m_originalVerts.Length; i++)
        {
            m_displacedVerts[i] = m_originalVerts[i];
            ogPoints[i] = new Vector4(m_originalVerts[i].x, m_originalVerts[i].y, m_originalVerts[i].z, 1.0f);
            vertex_damage[i] = 0.0f;
        }
        m_vertexVelocities = new Vector3[m_originalVerts.Length];
        forceIsApplied = false;


        var bounds = m_deformingMesh.bounds;

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.GetPropertyBlock(propertyBlock);
        rend.material.SetFloatArray("_Damage", vertex_damage);
        rend.material.SetVectorArray("ogPoints", ogPoints);
    }

    // Update is called once per frame
    void Update()
    {
        if (forceIsApplied)
        {
            for (int i = 0; i < m_displacedVerts.Length; i++)
            {
                Vector3 velocity = m_vertexVelocities[i];
                Vector3 displacement;
                // don't apply the spring force that would return the object to its origional shape
                // if we're holding the force.
                if (holdForce)
                {
                    displacement = new Vector3(0.0f, 0f, 0f);
                }
                // Snap the object back to shape
                else
                {
                    displacement = m_displacedVerts[i] - m_originalVerts[i];
                }
                velocity -= displacement * m_springForce * Time.deltaTime;
                velocity *= 1f - m_damping * Time.deltaTime;
                m_vertexVelocities[i] = velocity;
                m_displacedVerts[i] += velocity * Time.deltaTime;
            }
            m_deformingMesh.vertices = m_displacedVerts;
            m_deformingMesh.RecalculateNormals();
            meshCollider.sharedMesh = m_deformingMesh;

            m_deformingMesh.SetUVs(1, new List<Vector3>(originalMesh.vertices));
            rend.material.SetFloatArray("_Damage", vertex_damage);
        }
        var tmpNorm2 = new Vector2[m_deformingMesh.normals.Length];
        var tmpNorm1 = new Vector2[m_deformingMesh.normals.Length];
        for (int i = 0; i < m_deformingMesh.normals.Length; i++)
        {

            tmpNorm2[i] = Vector2.Max(new Vector2(m_deformingMesh.normals[i].x, m_deformingMesh.normals[i].y), m_deformingMesh.uv4[i]);
            tmpNorm1[i] = Vector2.Max(new Vector2(m_deformingMesh.normals[i].z, 0f), m_deformingMesh.uv5[i]);
        }
        m_deformingMesh.uv4 = tmpNorm2;
        m_deformingMesh.uv5 = tmpNorm1;
    }

    /**
     * Sets the deforming force to the mesh, deforming the mesh in the progress.
     *
     * @param forcePoint: The point at which the force is being applied.
     * @param forceAmount: The amount of force is being applied.
     */
    public void SetDeformingForce(Vector3 forcePoint, float forceAmount)
    {
        currentForcePoint = transform.InverseTransformPoint(forcePoint);
        currentFourceAmount = forceAmount;
        forceIsApplied = true;
        for (int i = 0; i < m_displacedVerts.Length; i++)
        {
            Vector3 pointToVertex = m_displacedVerts[i] - currentForcePoint;
            float attenuatedForce = currentFourceAmount / (1f + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.deltaTime;
            m_vertexVelocities[i] += pointToVertex.normalized * velocity;

            // Diff from original
            Vector3 displacement = m_displacedVerts[i] - m_originalVerts[i];

            // Diff from force point
            float mltp = Mathf.Max(1.0f, (m_originalVerts[i] - forcePoint).magnitude);

            vertex_damage[i] = NewVertDamage(vertex_damage[i], displacement.sqrMagnitude * (10 / mltp));
        }
    }

    /**
     * Calculates the new damage value.
     *
     * @param prev: The previous damage value.
     * @param force: The force used to determine the new deforming force.
     * @return: The new damage value.
     */
    public float NewVertDamage(float prev, float force)
    {

        float new_force = force / damage_threshold;

        // Take the max between them


        // And also limit it at 1
        return Mathf.Min(Mathf.Max(prev, new_force), 1.0f);
    }
}
