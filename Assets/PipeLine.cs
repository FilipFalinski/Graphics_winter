using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection.Emit;
using UnityEngine.Device;

public class PipeLine : MonoBehaviour
{
    private model f;
    private List<Vector3> verts;
    private float angle;
    private Vector3 axis;
    private Vector3 scale;
    private Vector3 translation;

    private Vector3 camPos;
    private Vector3 camLookAt;
    private Vector3 camUp;

    private Matrix4x4 transformMatrix;
    private Matrix4x4 viewingMatrix;
    private Matrix4x4 projectionMatrix;

    private List<Vector3> imageAfterTransform;
    private List<Vector3> imageAfterViewing;
    private List<Vector3> imageAfterProjection;
    private float z = 0.0f;
    private Texture2D screen;
    private Renderer screenPlane;

    private void Start()
    {
        screenPlane = FindObjectOfType<Renderer>();
        f = new model();
        CreateScreen();
        CalculateMatrices();
    }



    private void CalculateMatrices()
    {
        CalculateTransformMatrix();
        CalculateViewingMatrix();
        CalculateProjectionMatrix();
    }


    private void CalculateTransformMatrix()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-25, axis), Vector3.one);
        Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        Matrix4x4 translationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);

        transformMatrix = translationMatrix * scaleMatrix * rotationMatrix;
        imageAfterTransform = GetImageAfter(verts, transformMatrix);
    }



    private void CalculateViewingMatrix()
    {
        Matrix4x4 viewMatrix = Matrix4x4.LookAt(camPos, camLookAt, camUp);
        imageAfterViewing = GetImageAfter(imageAfterTransform, viewMatrix);
    }


    private void EverythingMatrix()
    {
        Matrix4x4 everythingMatrix = projectionMatrix * viewingMatrix * transformMatrix;
        List<Vector3> imageAfterEverything = GetImage(f.vertices, everythingMatrix);
    }

    private void CalculateProjectionMatrix()
    {
        Matrix4x4 projectionMatrix = Matrix4x4.Perspective(90, 1, 1, 1000);
        imageAfterProjection = GetImageAfter(imageAfterViewing, projectionMatrix);
    }


    private List<Vector3> GetImageAfter(List<Vector3> vertices, Matrix4x4 transformationMatrix)
    {
        List<Vector3> imgVertices = new List<Vector3>();

        foreach (Vector3 v in vertices)
        {
            Vector4 v2 = new Vector4(v.x, v.y, v.z, 1);
            imgVertices.Add(transformationMatrix * v2);
        }

        return imgVertices;
    }

    private void CreateScreen()
    {
        if (screen)
        {
            Destroy(screen);
        }

        screen = new Texture2D(256, 256);
        screenPlane.material.mainTexture = screen;
    }


    


/*   print_matrix(rotation_matrix);
   print_verts(image_after_rotation);
   print_matrix(scaleMatrix);
   print_verts(imageAfterScale);
   print_matrix(translationMatrix);
   print_verts(imageAfterTranslation);
   print_matrix(transformMatrix);
   print_verts(imageAfterTransform);
   print_matrix(viewMatrix);
   print_verts(imageAfterViewing);
   print(everythingMatrix);
   print_verts(imageAfterEverything);
   print_matrix(ProjectionMatrix);
   print_verts(imageAfterProjection);
   print_file2d(projectionByHand); */


private void Plot(List<Vector2Int> bresh)
{
    foreach (Vector2Int v in bresh)
    {
        screen.SetPixel(v.x, v.y, Color.red);
    }
}


private bool LineClip(ref Vector2 start, ref Vector2 end)
{
    Outcode startOutCode = new(start);
    Outcode endOutCode = new(end);
    Outcode inScreen = new();

    if ((startOutCode + endOutCode) == inScreen)
    {
        return true;
    }
    if ((startOutCode * endOutCode) != inScreen)
    {
        return false;
    }

    if (startOutCode == inScreen)
    {
        return LineClip(ref end, ref start);
    }

    List<Vector2> points = IntersectEdge(start, end, startOutCode);
    foreach (Vector2 v in points)
    {
        Outcode pointOutcode = new(v);
        if (pointOutcode == inScreen)
        {
            start = v;
            return LineClip(ref start, ref end);
        }
    }
    return false;
}


private List<Vector2> IntersectEdge(Vector2 start, Vector2 end, Outcode pointOutcode)
{
    float m = (end.y - start.y) / (end.x - start.x);
    List<Vector2> intersections = new();


    if (pointOutcode.up)
    {
        intersections.Add(new(start.x + (1 / m) * (1 - start.y), 1));
    }
    if (pointOutcode.down)
    {
        intersections.Add(new(start.x + (1 / m) * (-1 - start.y), -1));
    }
    if (pointOutcode.left)
    {
        intersections.Add(new(-1, start.y + m * (-1 - start.x)));
    }
    if (pointOutcode.right)
    {
        intersections.Add(new(1, start.y + m * (1 - start.x)));
    }

    return intersections;
}






    private List<Vector3> GetImage(List<Vector3> listVerts, Matrix4x4 transformMatrix)
    {
        List<Vector3> imgVerts = new List<Vector3>();

        foreach (Vector3 v in listVerts)
        {
            Vector4 v2 = new Vector4(v.x, v.y, v.z, 1);
            imgVerts.Add(transformMatrix * v2);
        }
        return imgVerts;
    }

    private List<Vector3> GetImageAfter()
{
    List<Vector3> vertices = f.vertices;
    Matrix4x4 translate = Matrix4x4.TRS(new Vector3(0, 0, 10), Quaternion.identity, Vector3.one);
    axis = new Vector3(16, 1, 1).normalized;
    Matrix4x4 rotate = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one);
    Matrix4x4 projection = Matrix4x4.Perspective(90, 1, 1, 1000);

    z += 0.5f;
    angle++;
    Matrix4x4 allTrans = projection * rotate * translate;

    return GetImage(vertices, allTrans);
}



private void print_matrix(Matrix4x4 matrix)
{
    string path = "Assets/matrix.txt";
    //Write some text to the test.txt file
    StreamWriter writer = new StreamWriter(path, true);

    for (int i = 0; i < 4; i++)
    {
        Vector4 row = matrix.GetRow(i);
        writer.WriteLine(row.x.ToString() + "   ,   " + row.y.ToString() + "   ,   " + row.z.ToString() + "   ,   " + row.w.ToString());


    }

    writer.Close();

}

private void print_verts(List<Vector3> v_list)
{
    string path = "Assets/vertices.txt";
    //Write some text to the test.txt file
    StreamWriter writer = new StreamWriter(path, true);
    foreach (Vector3 v in v_list)
    {
        writer.WriteLine(v.x.ToString() + "   ,   " + v.y.ToString() + "   ,   " + v.z.ToString() + "   ,   ");

    }
    writer.Close();
}
private void print_file2d(List<Vector2> points2d)
{
    string path = "Assets/2d.txt";
    //Write some text to the test.txt file
    StreamWriter writer = new StreamWriter(path, true);
    foreach (Vector2 v in points2d)
    {
        writer.WriteLine(v.x + "    ,   " + v.y);

    }
    writer.Close();
}

private void print_scale(List<Vector3> scale)
{
    string path = "Assets/scale.txt";
    //Write some text to the test.txt file
    StreamWriter writer = new StreamWriter(path, true);
    foreach (Vector3 v in scale)
    {
        writer.WriteLine(v.x + "    ,   " + v.y);

    }
    writer.Close();
}

private List<Vector2Int> Bresh(Vector2Int start, Vector2Int end)
{
    List<Vector2Int> points = new List<Vector2Int>();

    int dx = end.x - start.x;
    int dy = end.y - start.y;
    int p = 2 * dy - dx;

    if (dx < 0)
    {
        return Bresh(end, start);
    }
    if (dy < 0)
    {
        List<Vector2Int> negYResult = Bresh(new Vector2Int(start.x, -start.y), new Vector2Int(end.x, -end.y));
        return NegY(negYResult);
    }
    if (dy > dx)
    {
        List<Vector2Int> swappedResult = Bresh(new Vector2Int(start.y, start.x), new Vector2Int(end.y, end.x));
        return SwapXy(swappedResult);
    }

    int y = start.y;
    for (int x = start.x; x <= end.x; x++)
    {
        points.Add(new Vector2Int(x, y));
        if (p <= 0)
        {
            p += 2 * dy;
        }
        else
        {
            p += 2 * (dy - dx);
            y++;
        }
    }

    return points;
}

private void FloodFill(int x, int y)
{
    Stack<Vector2Int> pixels = new Stack<Vector2Int>();
    pixels.Push(new Vector2Int(x, y));

    while (pixels.Count > 0)
    {
        Vector2Int p = pixels.Pop();

        if (CheckBounds(p) && screen.GetPixel(p.x, p.y) != Color.red)
        {
            screen.SetPixel(p.x, p.y, Color.red);
            pixels.Push(new Vector2Int(p.x + 1, p.y));
            pixels.Push(new Vector2Int(p.x - 1, p.y));
            pixels.Push(new Vector2Int(p.x, p.y + 1));
            pixels.Push(new Vector2Int(p.x, p.y - 1));
        }
    }
}
private bool CheckBounds(Vector2Int pixel)
{
    return pixel.x < 0 || pixel.x >= screen.width || pixel.y < 0 || pixel.y >= screen.height;
}

private List<Vector2Int> NegY(List<Vector2Int> bresh)
{
    List<Vector2Int> breshFixed = new List<Vector2Int>();

    foreach (Vector2Int v in bresh)
    {
        breshFixed.Add(NegY(v));
    }

    return breshFixed;
}

private Vector2Int NegY(Vector2Int point)
{
    return new Vector2Int(point.x, -point.y);
}

private List<Vector2Int> SwapXy(List<Vector2Int> bresh)
{
    List<Vector2Int> breshFixed = new List<Vector2Int>();

    foreach (Vector2Int v in bresh)
    {
        breshFixed.Add(SwapXy(v));
    }

    return breshFixed;
}

private Vector2Int SwapXy(Vector2Int point)
{
    return new Vector2Int(point.y, point.x);
}

private Vector2Int Convert(Vector2 v)
{
    return new Vector2Int((int)(255 * (v.x + 1) / 2), (int)(255 * (v.y + 1) / 2));
}

private Vector2 DivideByZ(Vector3 input)
{
    return new Vector2(input.x / input.z, input.y / input.z);
}



}

