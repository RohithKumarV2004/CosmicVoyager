using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public static class CinemachineCameraSwitcher
{
    static List<CinemachineVirtualCamera> cameras=new List<CinemachineVirtualCamera>();

    static CinemachineVirtualCamera ActiveCamera;

    public static bool IsActiveCamera(CinemachineVirtualCamera camera){
        return camera==ActiveCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera camera){
        camera.Priority=10;
        ActiveCamera=camera;
        foreach (CinemachineVirtualCamera c in cameras){
            if(c!=camera){
                c.Priority=0;
            }
        }
    }

    public static void Register(CinemachineVirtualCamera camera){
        cameras.Add(camera);
        Debug.Log("camera registered "+camera);
    }
    public static void UnRegister(CinemachineVirtualCamera camera){
        cameras.Remove(camera);
        Debug.Log("camera unregistered "+camera);
    }
}
