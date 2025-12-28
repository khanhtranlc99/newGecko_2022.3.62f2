using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutGamePlay : TutorialFunController
{
    public bool isFirstBirdReady = false;
    
    /// <summary>
    /// Override OnBirdReady để xử lý khi bird ready
    /// Có thể gọi StartTut() từ đây nếu cần
    /// </summary>
    public override void OnBirdReady()
    {
        base.OnBirdReady();
        // Có thể thêm logic xử lý khi bird ready ở đây nếu cần
    }
}
