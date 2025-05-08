using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class InteractableItem : MonoBehaviour
{
public bool isnear=false;
 
void OnCollisionEnter2D(Collision2D other)
{
    //我们还增加了调试日志来了解飞弹触碰到的对象
    Debug.Log("Projectile Collision with " + other.gameObject);
    isnear=true;
}
 void OnCollisionExit2D(Collision2D other)
{
    //我们还增加了调试日志来了解飞弹触碰到的对象
    Debug.Log("Projectile leave" + other.gameObject);
    isnear=false;
}



    void Update()
    {  
        if (Input.GetMouseButtonDown(0)&&isnear==true) // 检测鼠标左键是否被按下
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 将鼠标屏幕坐标转换为世界坐标
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero); // 投射射线以检测碰撞
            
            if (hit.collider != null && hit.collider.gameObject == this.gameObject) // 检查射线是否击中当前对象
            {
                Debug.Log("Object Clicked!"); // 打印消息或执行其他操作
                // 这里可以添加其他交互逻辑，比如打开菜单、播放音效等
            }
        }
    }
}