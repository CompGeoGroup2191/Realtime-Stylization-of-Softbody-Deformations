using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    Mesh m_deformingMesh;
    Vector3[] m_originalVerts;
    Vector3[] m_displacedVerts;
    Vector3[] m_vertexVelocities;
    public float m_springForce = 20f;
    public float m_damping = 5f;

    public Mesh deformedMesh { get { return m_deformingMesh; } }
    public Vector3[] displacedVerts { get { return m_displacedVerts; } }
    public Vector3[] originalVerts { get { return m_originalVerts; } }

    // Start is called before the first frame update
    void Start()
    {
        m_deformingMesh = GetComponent<MeshFilter>().mesh;
        m_originalVerts = m_deformingMesh.vertices;
        m_displacedVerts = new Vector3[m_originalVerts.Length];
        for (int i = 0; i < m_originalVerts.Length; i++)
        {
            m_displacedVerts[i] = m_originalVerts[i];
        }
        m_vertexVelocities = new Vector3[m_originalVerts.Length];
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_displacedVerts.Length; i++)
        {
            UpdateVertex(i);
        }
        m_deformingMesh.vertices = m_displacedVerts;
        m_deformingMesh.RecalculateNormals();

    }

    // TODO: no more ugly xml comments
    /// <summary>
    /// Updates the velocity of the vertex at the given position and updates 
    /// the displaced vertex at the given position.
    /// </summary>
    /// <param name="i"> The index that will be used to update a vertex. </param>
    void UpdateVertex(int i)
    {
        Vector3 velocity = m_vertexVelocities[i];
        Vector3 displacement = m_displacedVerts[i] - m_originalVerts[i];
        velocity -= displacement * m_springForce * Time.deltaTime;
        velocity *= 1f - m_damping * Time.deltaTime;
        m_vertexVelocities[i] = velocity;
        m_displacedVerts[i] += velocity * Time.deltaTime;
    }

    /// <summary>
    /// Uses the given point and force value to add a deforming force to the mesh.
    /// </summary>
    /// <param name="point"> The point at which the force is being applied. </param>
    /// <param name="force"> The amount of force being applied. </param>
    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < m_displacedVerts.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    /// <summary>
    /// Calculates the velocity for the given vertex index using the point at which force is 
    /// applied and the force.
    /// </summary>
    /// <param name="i"> The index of the vertex that we're calculating the velocity of. </param>
    /// <param name="point"> The point at which the force is being applied. </param>
    /// <param name="force"> The amount of force being applied. </param>
    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = m_displacedVerts[i] - point;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        m_vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
}
