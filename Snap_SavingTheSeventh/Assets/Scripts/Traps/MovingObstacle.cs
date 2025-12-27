using UnityEngine;
using System.Collections;
// import { deleteBookAction } from "@/lib/actions";

public class MovingObstacle : MonoBehaviour
{
    public float speed;
    Vector3 targetPos;
    public float delayTime = 1f;

    public float initialStartDelay = 0f;
    public int startAtIndex = 1;

    public GameObject ways;
    public Transform[] wayPoints;
    int pointIndex;
    int pointCount;
    int direction = 1;
    bool isDelay = false;

    private void Awake()
    {
        wayPoints = new Transform[ways.transform.childCount];
        for(int i = 0; i < ways.gameObject.transform.childCount; i++)
        {
            wayPoints[i] = ways.transform.GetChild(i).gameObject.transform;
        }   
    }   

    private IEnumerator Start()
    {
        pointCount = wayPoints.Length;
        
        // Validasi biar ga error kalau kamu masukin angka aneh di inspector
        if (startAtIndex >= pointCount) startAtIndex = pointCount - 1;
        if (startAtIndex < 0) startAtIndex = 0;

        pointIndex = startAtIndex; // Set start index sesuai settingan

        // Langsung teleport ke posisi awal (biar ga melayang dari posisi random)
        // Kalau mau dia jalan dari posisi spawn ke titik pertama, hapus baris ini:
        transform.position = wayPoints[pointIndex].transform.position; 
        
        // Tentukan target berikutnya
        CalculateNextTarget();

        // FITUR DELAY AWAL: Nunggu dulu sebelum mulai loop update
        if (initialStartDelay > 0)
        {
            isDelay = true; // Kunci pergerakan
            yield return new WaitForSeconds(initialStartDelay);
            isDelay = false; // Buka kunci
        }
    }

    private void Update()
    {
        if (isDelay) return;
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (transform.position == targetPos)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    IEnumerator WaitAndMove()
    {
        isDelay = true; 
        
        yield return new WaitForSeconds(delayTime); 

        CalculateNextTarget(); // Panggil fungsi hitung target
        
        isDelay = false; 
    }

    void CalculateNextTarget()
    {
        // Cek ujung array untuk balik arah
        if (pointIndex >= pointCount - 1)
        {
            direction = -1;
        }
        else if (pointIndex <= 0)
        {
            direction = 1;
        }

        // Pindah ke index berikutnya
        pointIndex += direction;
        
        // Set target posisi
        targetPos = wayPoints[pointIndex].transform.position;
    }
}
