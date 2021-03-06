using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ArcAlignment
{
    None,
    Straight,
    XZCurve,
    YCurve,
    YZCurve,
    XCurve,
    ZCurve
}
public class Directive
{
    /// <summary>
    /// The manner in which the points in front of this directive are aligned
    /// </summary>
    public ArcAlignment Alignment;
    /// <summary>
    /// The direction this directive gives to the copter
    /// </summary>
    public Vector3 LookVector { get; set; }

    /// <summary>
    /// The list of points in front of this directive
    /// </summary>
    public List<Vector3> Points { get; set; }

    /// <summary>
    /// The speed the copter will use while flying through this directives set of points
    /// </summary>
    public float Speed;

    /// <summary>
    /// Distance from this directive to the next directive
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// This directive's referenced point in world space
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Visual representation of the directive.
    /// </summary>
    public GameObject Pyramid { get; set; }

    /// <summary>
    /// The time this directive commands the copter to wait, in seconds
    /// </summary>
    /// <value>The wait time.</value>
    public float WaitTime = 0.0f;

    public bool LookEdit = false;
	public LineRenderer Line;
	public int VertexCount;
    // Use this for initialization
    void Start()
    {
        this.Line = this.Pyramid.GetComponent<LineRenderer>();
        InitVariables();
    }

	int index; // the index used to move the particle system along this line
    // Update is called once per frame - NOT TRUE
    public void Update(ParticleSystem PS)
    {
        if (Highlight) 
		{   
			Pyramid.renderer.material.color = new Color(0.3f, 1.0f, 0.3f);
			Line.material.color = Color.white;
			PS.enableEmission = (index != 0);
			Vector3 direction = Points [index] - PS.transform.position;
			Vector3 next = direction.normalized * Time.deltaTime * Speed * 20f;
			float dmag = direction.magnitude;
			if(next.magnitude > dmag)
			{
				PS.transform.position = Points[index];
				index = (index + 1) % Points.Count;
			}
			else 
			{
				PS.transform.position += next;
				if (dmag < 0.2f) 
					index = (index + 1) % Points.Count;
			}
		} 
		else
		{
			Line.material.color = new Color(0.245f, 1f, 0f);
			Pyramid.renderer.material.color = new Color(0.245f, 1f, 0f);
			index = 0;
		}
    }
    /// <summary>
    /// Creates a directive at the given points, with a look and distance value
    /// </summary>
    /// <param name="position"></param>
    /// <param name="lastPosition"></param>
    public Directive(Vector3 position, GameObject pyramid)
    {
        InitVariables();
        this.Position = position;
        this.Pyramid = pyramid;
        this.Pyramid.transform.position = position;
        this.Line = this.Pyramid.GetComponent<LineRenderer>();
		VertexCount++;
		Line.SetVertexCount(VertexCount);
		Line.SetPosition (VertexCount - 1, position);
        this.Points.Add(position);
    }
    public void Set(Vector3 position, int id, List<Directive> dirs)
    {
        this.Position = position;
        this.Points[0] = position;
        this.Pyramid.transform.position = position;
		Line.SetPosition (0, Position);
        Line.enabled = true;
		if (id > 0)
			dirs[id - 1].Line.SetPosition (dirs[id - 1].VertexCount - 1, Position);
        FindDistanceToNextDirective();
    }
	public void AddPoint(Vector3 p) 
	{
		Points.Add (p);
		Distance += Vector3.Distance(
			Points[Points.Count - 2],
			Points[Points.Count - 1]);
		VertexCount++;      
		Line.SetVertexCount(VertexCount);
		Line.SetPosition(VertexCount - 1, p);
	}

	public void FindDistanceToNextDirective()
	{
		float d = 0f;
		for (int i = 1; i < Points.Count; i++)
			d += Vector3.Distance(Points[i], Points[i - 1]);
        Distance = d;
    }
    private void AlignToMe(Directive me)
    {
        if (Alignment == ArcAlignment.None)
            return;
        Vector3 endPoint = me.Points[0];
        Vector3 startPoint = Points[0];
        AlignmentSwitch(startPoint, endPoint);
    }
    public void Align(List<Directive> dirs, int id)
    {
        if (Alignment == ArcAlignment.None || Points.Count == 1)
        {
            if (id > 0)
            {
                dirs[id - 1].AlignToMe(this);
            }
            Points[0] = Position;
			Line.SetPosition (0, Position);
            return;
        }
        int nextid = id < dirs.Count - 1 ? id + 1 : -1;
        Vector3 endPoint = nextid > -1 ? dirs[nextid].Points[0] : Points[Points.Count - 1];
        Vector3 startPoint = Points[0];

        AlignmentSwitch(startPoint, endPoint);
        SetEndPoints(nextid, endPoint, id, dirs);
        Points[0] = Position;
		Line.SetPosition (0, Position);
    }
    void AlignmentSwitch(Vector3 startPoint, Vector3 endPoint)
    {
        Bezier b = new Bezier(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
        Vector3 dif = new Vector3((startPoint.x - endPoint.x) / 2, (startPoint.y - endPoint.y) / 2, (startPoint.z - endPoint.z) / 2);
        switch (Alignment)
        {
            case ArcAlignment.Straight:
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Points[i] = Vector3.Lerp(Position, endPoint, n);
					Line.SetPosition(i, Points[i]);
                }
                break;
            case ArcAlignment.YZCurve:
                dif.x = 0;
                b = new Bezier(startPoint, -dif, dif, endPoint);
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Vector3 newPoint = new Vector3(Points[i].x, b.GetPointAtTime(n).y, b.GetPointAtTime(n).z);
                    Points[i] = newPoint;
					Line.SetPosition(i, Points[i]);
                }
                break;
            case ArcAlignment.XZCurve:
                dif.y = 0;
                b = new Bezier(startPoint, -dif, dif, endPoint);
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Vector3 newPoint = new Vector3(b.GetPointAtTime(n).x, Points[i].y, b.GetPointAtTime(n).z);
                    Points[i] = newPoint;
					Line.SetPosition(i, Points[i]);
                }
                break;
            case ArcAlignment.XCurve:
                dif.y = 0; dif.z = 0;
                b = new Bezier(startPoint, -dif, dif, endPoint);
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Vector3 newPoint = new Vector3(b.GetPointAtTime(n).x, Points[i].y, Points[i].z);
                    Points[i] = b.GetPointAtTime(n);;
					Line.SetPosition(i, Points[i]);
                }
                break;
            case ArcAlignment.YCurve:
                dif.x = 0; dif.z = 0;
                b = new Bezier(startPoint, -dif, dif, endPoint);
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Vector3 newPoint = new Vector3(Points[i].x, b.GetPointAtTime(n).y, Points[i].z);
                    Points[i] = newPoint;
					Line.SetPosition(i, Points[i]);
                }
                break;
            case ArcAlignment.ZCurve:
                dif.x = 0; dif.y = 0;
                b = new Bezier(startPoint, -dif, dif, endPoint);
                for (int i = 0; i < Points.Count; i++)
                {
                    float n = (float)(i) / (float)(Points.Count - 1);
                    Vector3 newPoint = new Vector3(Points[i].x, Points[i].y, b.GetPointAtTime(n).z);
                    Points[i] = newPoint;
					Line.SetPosition(i, Points[i]);
                }
                break;
            default:
                break;
        }
        FindDistanceToNextDirective();
    }
    void SetEndPoints(int nextid, Vector3 endPoint, int id, List<Directive> dirs)
    {
        if (nextid > -1)
        {
            dirs[nextid].Position = endPoint;
            dirs[nextid].Points[0] = endPoint;
			Line.SetPosition(Points.Count - 1, Points[Points.Count - 1]);
        }
        if (id > 0)
            dirs[id - 1].AlignToMe(this);
    }

    void InitVariables()
    {
		VertexCount = 0;
        LookVector = Vector3.forward;
        Speed = 1.0f;
        Distance = 0;
        WaitTime = 0.0f;
        Points = new List<Vector3>();
        Alignment = ArcAlignment.None;
    }
    // Determines if this directive is highlighted
	public bool Highlight { get; set; }
}
public class Bezier
{
    // thanks Loran from the Unity Forums
    // http://forum.unity3d.com/threads/5082-Bezier-Curve

    Vector3 p0;
    Vector3 p1;
    Vector3 p2;
    Vector3 p3;

    private Vector3 b0 = Vector3.zero;
    private Vector3 b1 = Vector3.zero;
    private Vector3 b2 = Vector3.zero;
    private Vector3 b3 = Vector3.zero;

    private float Ax;
    private float Ay;
    private float Az;

    private float Bx;
    private float By;
    private float Bz;

    private float Cx;
    private float Cy;
    private float Cz;

    // Init function v0 = 1st point, v1 = handle of the 1st point , v2 = handle of the 2nd point, v3 = 2nd point
    // handle1 = v0 + v1
    // handle2 = v3 + v2
    public Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.p0 = v0;
        this.p1 = v1;
        this.p2 = v2;
        this.p3 = v3;
    }

    // 0.0 >= t <= 1.0
    public Vector3 GetPointAtTime(float t)
    {
        this.CheckConstant();
        float t2 = t * t;
        float t3 = t * t * t;
        float x = this.Ax * t3 + this.Bx * t2 + this.Cx * t + p0.x;
        float y = this.Ay * t3 + this.By * t2 + this.Cy * t + p0.y;
        float z = this.Az * t3 + this.Bz * t2 + this.Cz * t + p0.z;
        return (new Vector3(x, y, z));

    }

    private void SetConstant()
    {
        this.Cx = 3 * ((this.p0.x + this.p1.x) - this.p0.x);
        this.Bx = 3 * ((this.p3.x + this.p2.x) - (this.p0.x + this.p1.x)) - this.Cx;
        this.Ax = this.p3.x - this.p0.x - this.Cx - this.Bx;

        this.Cy = 3 * ((this.p0.y + this.p1.y) - this.p0.y);
        this.By = 3 * ((this.p3.y + this.p2.y) - (this.p0.y + this.p1.y)) - this.Cy;
        this.Ay = this.p3.y - this.p0.y - this.Cy - this.By;

        this.Cz = 3 * ((this.p0.z + this.p1.z) - this.p0.z);
        this.Bz = 3 * ((this.p3.z + this.p2.z) - (this.p0.z + this.p1.z)) - this.Cz;
        this.Az = this.p3.z - this.p0.z - this.Cz - this.Bz;

    }

    // Check if p0, p1, p2 or p3 have changed
    private void CheckConstant()
    {
        if (this.p0 != this.b0 || this.p1 != this.b1 || this.p2 != this.b2 || this.p3 != this.b3)
        {
            this.SetConstant();
            this.b0 = this.p0;
            this.b1 = this.p1;
            this.b2 = this.p2;
            this.b3 = this.p3;
        }
    }
}
