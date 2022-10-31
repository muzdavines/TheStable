version 1.5.0

email is "kripto289@gmail.com" 
Discord link https://discord.gg/GUUZ9D96Uq 



!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   FIRST STEPS  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

1) All effects are configured for HDR rendering with bloom postprocessing!

2) Settings for STANDARD (non URP or HDRP) rendering:
http://kripto289.com/Shared/Readme/DefaultRenderingSettings.jpg

3) Settings for URP rendering:
http://kripto289.com/Shared/Readme/URPRenderingSettings.jpg
You should import the URP patch from the folder 
(\Assets\KriptoFX\MeshEffect\HDRP and URP patches)

4) Settings for HDRP rendering:
HDRP rendering has correct settings out of the box and you only need to import the patch from the folder 
(\Assets\KriptoFX\MeshEffect\HDRP and URP patches)

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------





-----------------------------------------      EFFECTS USING    ----------------------------------------------------------------------------------------------------------

If your model (mesh) has several materials, then the effect will be applied only to the first material. To fix this, split your mesh. 1 mesh = 1 material.

Using in editor mode:

	1) Just drag the effect prefab onto the scene (the effect should be nested in the hierarchy of mesh, for example Cube(root) -> Effect1(child))
	2) CLick on this effect prefab (in the scene hierarhy) -> set the "Mesh Object" of the script "PSMeshRendererUpdater".
	3) Click "Update Mesh Renderer".
	Particles and materials will be added to your object automatically. But if you want, you can add material and particles manually and ignore all steps above. 


In runtime mode:

	var currentInstance = Instantiate(Effect) as GameObject; 
	var psUpdater = currentInstance.GetComponent<PSMeshRendererUpdater>();
	psUpdater.UpdateMeshEffect(MeshObject);


For SCALING just change transform scale of mesh or use "StartScaleMultiplier" of script "PSMeshRendererUpdater"

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------