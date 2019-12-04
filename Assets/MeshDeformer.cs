using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    /**
     * BIG NOTE: For this to work the object that it's attached to has to use a mesh collider.
     */
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
    // Renderer rend;

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
        // originalMesh = new Mesh();
        originalMesh = Instantiate(m_deformingMesh);

        // originalMesh.vertices = new Vector3[m_deformingMesh.vertices.Length];
        // originalMesh.vertices = new Vector3[m_deformingMesh.vertices.Length];

        // for (int i = 0; i < m_deformingMesh.vertices.Length; i++)
        // {
        //     originalMesh.vertices[i] = m_deformingMesh.vertices[i];
        // }
        originalMesh.RecalculateNormals();
        var tmpVec2 = new Vector2[m_deformingMesh.vertices.Length];
        var tmpVec1 = new Vector2[m_deformingMesh.vertices.Length];

        var tmpNorm2 = new Vector2[m_deformingMesh.normals.Length];
        var tmpNorm1 = new Vector2[m_deformingMesh.normals.Length];
        // m_deformingMesh.SetUVs(1, new List<Vector3>(originalMesh.vertices));
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
        m_deformingMesh.uv2 = tmpVec2;
        m_deformingMesh.uv3 = tmpVec1;
        m_deformingMesh.uv4 = tmpNorm2;
        m_deformingMesh.uv5 = tmpNorm1;
        // m_deformingMesh.SetUVs(1, new List<Vector2>(tmpVec2));
        // m_deformingMesh.SetUVs(2, new List<Vector2>(tmpVec1));

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

        // Grab the renderer, assume that the material is the correct shader
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.GetPropertyBlock(propertyBlock);
        // propertyBlock.SetVectorArray("OriginalVertecies", ogPoints);
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

                    // vertex_damage[i] = Mathf.Max(vertex_damage[i], displacement.magnitude * 3.0f);
                }
                velocity -= displacement * m_springForce * Time.deltaTime;
                velocity *= 1f - m_damping * Time.deltaTime;
                m_vertexVelocities[i] = velocity;
                m_displacedVerts[i] += velocity * Time.deltaTime;
            }
            m_deformingMesh.vertices = m_displacedVerts;
            m_deformingMesh.RecalculateNormals();
            //m_deformingMesh.RecalculateBounds();
            meshCollider.sharedMesh = m_deformingMesh;

            // Following the deformation, style the material
            // rend.material.SetColor("_Color", new Color(0.5f, 0.1f, 0.1f, 1.0f));
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

    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < m_displacedVerts.Length; i++)
        {
            Vector3 pointToVertex = m_displacedVerts[i] - point;
            float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.deltaTime;
            // if (attenuatedForce > force) {

            //     m_vertexVelocities[i] += pointToVertex.normalized * velocity;
            // }
            m_vertexVelocities[i] += pointToVertex.normalized * velocity;
        }
    }

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
            // float mltp = Mathf.Max(1.0f, (forcePoint - m_originalVerts[i]).sqrMagnitude);
            float mltp = Mathf.Max(1.0f, (m_originalVerts[i] - forcePoint).magnitude);
            // Vector3 
            // Debug.Log(displacement);
            // Debug.Log(mltp);

            vertex_damage[i] = NewVertDamage(vertex_damage[i], displacement.sqrMagnitude * (10 / mltp));
        }
    }

    public float NewVertDamage(float prev, float force)
    {

        float new_force = force / damage_threshold;

        // Take the max between them


        // And also limit it at 1
        return Mathf.Min(Mathf.Max(prev, new_force), 1.0f);
    }
}
