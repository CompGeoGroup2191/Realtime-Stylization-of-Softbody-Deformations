using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    /**
     * BIG NOTE: For this to work the object that it's attached to has to use a mesh collider.
     */
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
    float currentFourceAmount;

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
        meshCollider = GetComponent<MeshCollider>();
        m_originalVerts = m_deformingMesh.vertices;
        m_displacedVerts = new Vector3[m_originalVerts.Length];
        for (int i = 0; i < m_originalVerts.Length; i++)
        {
            m_displacedVerts[i] = m_originalVerts[i];
        }
        m_vertexVelocities = new Vector3[m_originalVerts.Length];
        forceIsApplied = false;


        var bounds = m_deformingMesh.bounds;


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
            //m_deformingMesh.RecalculateBounds();
            meshCollider.sharedMesh = m_deformingMesh;
        }
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < m_displacedVerts.Length; i++)
        {
            Vector3 pointToVertex = m_displacedVerts[i] - point;
            float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.deltaTime;
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
        }
    }
}
