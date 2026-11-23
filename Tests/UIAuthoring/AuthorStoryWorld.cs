using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Executado apenas na cópia de autoria; as cenas resultantes são copiadas após validação.
public static class AuthorStoryWorld
{
    static TMP_FontAsset font;
    static PlayerClickMovement player;
    static Transform canvas;
    static readonly string[] scenes = { "Beach", "House", "Island", "Cave", "Lighthouse" };
    static readonly string[] art = { "01-praia", "02-casa", "03-centro", "04-caverna", "05-farol" };
    static GameObject Named(string name) => Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(x => x.name == name);
    static void Remove(string name) { var go = Named(name); if (go != null) Object.DestroyImmediate(go); }
    static void Disable(string name) { var go = Named(name); if (go != null) go.SetActive(false); }
    static void Set(Object owner, string field, Object value) { var s = new SerializedObject(owner); s.FindProperty(field).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Str(Object owner,string field,string value) {var s=new SerializedObject(owner);s.FindProperty(field).stringValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Float(Object owner,string field,float value) {var s=new SerializedObject(owner);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static GameObject Ref(Object owner,string field) => (GameObject)new SerializedObject(owner).FindProperty(field).objectReferenceValue;
    static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static TMP_Text Text(string name,Transform parent,string text,float x,float y,float w,float h,float size)
    {
        var t=Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSharedMaterial=font.material;t.text=text;t.fontSize=size;t.color=new Color(.96f,.94f,.85f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;
    }
    static Transform Point(Transform parent,Vector3 p) {var go=new GameObject("Approach");go.transform.SetParent(parent);go.transform.position=p;return go.transform;}
    static void Hit(GameObject go,Vector3 center,Vector3 size)
    {
        foreach(var c in go.GetComponents<Collider>()) Object.DestroyImmediate(c);
        var b=go.AddComponent<BoxCollider>();b.isTrigger=true;b.center=go.transform.InverseTransformPoint(center);b.size=size;
    }
    static void Door(string destination,string label,Vector3 click,Vector3 approach,Vector3 size)
    {
        var go=new GameObject("PathTo"+destination,typeof(DoorInteraction));go.transform.position=click;
        var door=go.GetComponent<DoorInteraction>();Str(door,"sceneName",destination);Str(door,"actionLabel",label);Float(door,"interactionDistance",.18f);Set(door,"approachPoint",Point(go.transform,approach));Hit(go,click,size);
        // Pequena legenda no mundo torna todas as saídas descobríveis sem depender de hover.
        var marker=new GameObject("DestinationLabel",typeof(TextMeshPro));marker.transform.SetParent(go.transform);marker.transform.position=approach+new Vector3(0,-.4f,-.15f);
        var text=marker.GetComponent<TextMeshPro>();text.font=font;text.text=label+"  ›";text.fontSize=2.8f;text.color=new Color(1,.87f,.58f);text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(3,.5f);text.sortingOrder=15;marker.AddComponent<BoxCollider>().size=new Vector3(3,.6f,.3f);
    }
    static void ConfigureInteraction(Interactable interaction,string label,Vector3 click,Vector3 approach,Vector3 size)
    {
        interaction.transform.position=click;Set(interaction,"player",player.transform);Set(interaction,"interactionPrompt",null);Set(interaction,"approachPoint",Point(interaction.transform,approach));Str(interaction,"actionLabel",label);Float(interaction,"interactionDistance",.3f);Hit(interaction.gameObject,click,size);
    }
    public static void Run()
    {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        foreach(string a in art) {var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/Backgrounds/Story/"+a+".png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
        if (!System.IO.File.Exists("Assets/Editor/StoryTemplate.unity")) AssetDatabase.CopyAsset("Assets/Scenes/Island.unity","Assets/Editor/StoryTemplate.unity");
        if (!System.IO.File.Exists("Assets/Editor/ReturnPanel.prefab")) {
        EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
        var returnInteraction=Object.FindAnyObjectByType<ScrollInteraction>();
        PrefabUtility.SaveAsPrefabAsset(Ref(returnInteraction,"scrollPanel"),"Assets/Editor/ReturnPanel.prefab");
        }
        for(int i=0;i<scenes.Length;i++) Build(i);
        BuildEnding();
        var menuScene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        foreach(var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(label.text.Contains("MOUSE ESQUERDO") && label.text.Contains("Clique no")) label.text="W A S D\nMover o personagem.\n\nMOUSE ESQUERDO\nInteragir com objetos e caminhos.\n\nI  Inventário     ESC  Pausa";
        EditorSceneManager.SaveScene(menuScene);
        EditorBuildSettings.scenes=new[]{"Splash","MainMenu","Beach","House","Island","Cave","Lighthouse","Ending","Credits"}.Select(x=>new EditorBuildSettingsScene("Assets/Scenes/"+x+".unity",true)).ToArray();
        AssetDatabase.SaveAssets();Debug.Log("STORY WORLD AUTHORED");EditorApplication.Exit(0);
    }
    static void Build(int index)
    {
        string name=scenes[index];var scene=EditorSceneManager.OpenScene("Assets/Editor/StoryTemplate.unity");
        player=Named("Player").GetComponent<PlayerClickMovement>();canvas=Named("GameplayMenusCanvas").transform;
        var camera=Camera.main;camera.transform.SetPositionAndRotation(new Vector3(0,0,-10),Quaternion.identity);camera.orthographic=true;camera.orthographicSize=5;
        foreach(var depth in Object.FindObjectsByType<PlayerDepth>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Object.DestroyImmediate(depth);
        foreach(var obstacle in Object.FindObjectsByType<CustomObstacleMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Object.DestroyImmediate(obstacle.gameObject);
        foreach(var door in Object.FindObjectsByType<DoorInteraction>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Object.DestroyImmediate(door.gameObject);
        foreach(var oldBackground in scene.GetRootGameObjects().Where(x=>x.name=="Background")) Object.DestroyImmediate(oldBackground);
        Remove("WalkableArea");
        var bg=new GameObject("StoryBackground",typeof(SpriteRenderer));bg.transform.position=new Vector3(0,0,2);
        var renderer=bg.GetComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Story/"+art[index]+".png");renderer.sortingOrder=-100;bg.transform.localScale=Vector3.one*(10/renderer.sprite.bounds.size.y);
        var floor=new GameObject("WalkableArea",typeof(BoxCollider));floor.transform.position=new Vector3(0,-2,1);floor.GetComponent<BoxCollider>().size=new Vector3(16,4,.1f);
        player.transform.SetPositionAndRotation(new Vector3(0,-3,-1),Quaternion.identity);player.transform.localScale=Vector3.one;
        var ps=new SerializedObject(player);ps.FindProperty("keyboardMovement").boolValue=true;ps.FindProperty("walkMin").vector2Value=new Vector2(-7.4f,-4.1f);ps.FindProperty("walkMax").vector2Value=new Vector2(7.4f,-.45f);ps.FindProperty("obstacleLayer").intValue=0;ps.ApplyModifiedPropertiesWithoutUndo();
        foreach(var sprite in player.GetComponentsInChildren<SpriteRenderer>()) {sprite.sortingOrder=5;}
        foreach(string old in new[]{"CliqueChao","InteractionPrompt","LighthousePrompt","MiningAreaHint"}) Disable(old);
        var results=Object.FindAnyObjectByType<MissionResultUI>();var rs=new SerializedObject(results);rs.FindProperty("cinematicEnding").boolValue=true;rs.ApplyModifiedPropertiesWithoutUndo();
        var top=Rect("StoryObjectiveCard",canvas,0,0,960,88);top.anchorMin=top.anchorMax=top.pivot=new Vector2(.5f,1);top.anchoredPosition=new Vector2(0,-12);top.gameObject.AddComponent<Image>().color=new Color(.035f,.075f,.09f,.92f);top.GetComponent<Image>().raycastTarget=false;
        var objective=Text("StoryObjective",top,"",0,0,930,80,22);
        var bottom=Rect("StoryControlsBar",canvas,0,0,1280,42);bottom.anchorMin=bottom.anchorMax=bottom.pivot=new Vector2(.5f,0);bottom.anchoredPosition=Vector2.zero;bottom.gameObject.AddComponent<Image>().color=new Color(.035f,.075f,.09f,.9f);bottom.GetComponent<Image>().raycastTarget=false;
        var inventory=Object.FindAnyObjectByType<CrystalInventoryUI>();Disable("InventoryKeyHint");Set(inventory,"keyHint",null);
        var controls=Text("StoryControls",canvas,"W A S D  mover     •     Clique para interagir     •     I  inventário     •     Esc  pausa",0,0,1000,30,18);controls.rectTransform.anchorMin=controls.rectTransform.anchorMax=new Vector2(.5f,0);controls.rectTransform.anchoredPosition=new Vector2(0,18);
        var hover=Text("StoryHover",canvas,"",0,0,900,38,24);hover.rectTransform.anchorMin=hover.rectTransform.anchorMax=new Vector2(.5f,0);hover.rectTransform.anchoredPosition=new Vector2(0,58);Set(player,"hoverText",hover);
        foreach(var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(label.text.Contains("MOUSE ESQUERDO") && label.text.Contains("Clique no")) label.text="W A S D\nMover o personagem.\n\nMOUSE ESQUERDO\nInteragir com objetos e caminhos.\n\nI  Inventário     ESC  Pausa";
            if(label.text.Contains("Converse com o estranho sobre a tempestade")) label.text="Volte à casa e releia o bilhete sobre a mesa.";
            if(label.text.Contains("parte inferior direita")) label.text="Clique nos veios para minerar. I abre o inventário.";
        }
        var location=new GameObject("StoryLocation",typeof(StoryLocation)).GetComponent<StoryLocation>();Set(location,"player",player);Set(location,"objective",objective);
        var loc=new SerializedObject(location);var arrivals=loc.FindProperty("arrivals");arrivals.arraySize=0;loc.ApplyModifiedPropertiesWithoutUndo();
        var npc=Object.FindAnyObjectByType<NPCInteraction>();
        if(name!="Island") {if(npc!=null) Object.DestroyImmediate(npc.transform.root.gameObject);Remove("DialogueController");Disable("DialoguePanel");}
        else {
            npc.transform.root.position=new Vector3(1.1f,-.4f,-1);npc.transform.root.localScale=Vector3.one*.8f;
            ConfigureInteraction(npc,"Conversar com o estranho",new Vector3(1.1f,-.4f,-1),new Vector3(1.1f,-1.3f,-1),new Vector3(1.4f,2,.3f));
        }
        var scroll=Object.FindAnyObjectByType<ScrollInteraction>();
        if(name!="House") {Ref(scroll,"scrollPanel").SetActive(false);scroll.gameObject.SetActive(false);}
        else {
            ConfigureInteraction(scroll,"Ler o bilhete da casa",new Vector3(.1f,1.5f,-1),new Vector3(.1f,-.6f,-1),new Vector3(3,2,.3f));
            var panel=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/ReturnPanel.prefab"),canvas);panel.name="ReturnMemoryPanel";
            Set(scroll,"returnPanel",panel);panel.SetActive(false);
            var close=panel.transform.Find("CloseButton").GetComponent<Button>();close.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(close.onClick,scroll.CloseScroll);
            var returnPuzzle=panel.GetComponent<SymbolSequencePuzzle>();Str(returnPuzzle,"openingClue","Talvez as lembranças revelem o sentido deste bilhete.");Str(returnPuzzle,"nextStepText","Siga do centro até o farol e acenda a luz.");
            var first=Ref(scroll,"scrollPanel").GetComponent<SymbolSequencePuzzle>();Str(first,"nextStepText","Siga para a caverna e procure os três cristais.");
            foreach(var t in panel.GetComponentsInChildren<TMP_Text>(true)) if(t.text=="O BILHETE DO ABRIGO")t.text="O BILHETE DA VOLTA";
        }
        var mine=Object.FindAnyObjectByType<MiningSpot>();var miningUI=Object.FindAnyObjectByType<MiningUI>();
        if(name!="Cave") {mine.gameObject.SetActive(false);miningUI.enabled=false;foreach(string n in new[]{"MiningPanel","WorldMiningPanel","MiningAreaHint"})Disable(n);}
        else {
            mine.transform.position=new Vector3(0,-.6f,-1);Float(mine,"interactionDistance",7.8f);Set(mine,"approachPoint",Point(mine.transform,new Vector3(-4,-.6f,-1)));
            foreach(var mesh in mine.GetComponentsInChildren<MeshRenderer>(true)) if(!mesh.transform.IsChildOf(Named("MiningPickaxe").transform))mesh.enabled=false;
            foreach(var c in mine.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var position in new[]{new Vector3(-4,1.5f,-1),new Vector3(4,1.5f,-1),new Vector3(6.6f,.8f,-1)}) {
                var hit=new GameObject("MineralVein");hit.transform.SetParent(mine.transform);hit.transform.position=position;hit.AddComponent<BoxCollider>().size=new Vector3(2,2,.3f);
            }
            var sockets=new GameObject("CrystalSockets");sockets.transform.SetParent(mine.transform);sockets.transform.position=new Vector3(0,1.3f,-1);var socketHit=sockets.AddComponent<BoxCollider>();socketHit.size=new Vector3(3,1.5f,.3f);Set(mine,"socketCollider",socketHit);
            Disable("MiningAreaHint");
        }
        var lighthouse=Object.FindAnyObjectByType<LighthouseInteraction>();
        if(name!="Lighthouse") lighthouse.gameObject.SetActive(false);
        else {
            ConfigureInteraction(lighthouse,"Acender a luz do farol",new Vector3(0,1.1f,-1),new Vector3(0,-.8f,-1),new Vector3(3,3,.3f));
            var beacon=Ref(lighthouse,"beacon");beacon.transform.SetParent(null);beacon.transform.position=new Vector3(0,2.5f,-.7f);beacon.transform.localScale=Vector3.one*1.8f;
            foreach(var mesh in lighthouse.GetComponentsInChildren<MeshRenderer>(true))mesh.enabled=false;
            var panel=Ref(lighthouse,"panel");((RectTransform)panel.transform).sizeDelta=new Vector2(760,530);
            var text=(TMP_Text)new SerializedObject(lighthouse).FindProperty("statusText").objectReferenceValue;text.rectTransform.sizeDelta=new Vector2(700,190);text.rectTransform.anchoredPosition=new Vector2(0,30);text.fontSize=25;
            Named("LighthouseTitle").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,215);
            Named("ActivateLighthouse").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-145);Named("ActivateLighthouse").GetComponentInChildren<TMP_Text>().text="INICIAR / TENTAR NOVAMENTE";
            Named("CloseLighthouse").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-215);Named("CloseLighthouse").GetComponentInChildren<TMP_Text>().text="FECHAR";
            var bar=Rect("QteTime",panel.transform,0,-100,590,16);bar.gameObject.AddComponent<Image>().color=new Color(.2f,.27f,.3f);var slider=bar.gameObject.AddComponent<Slider>();slider.interactable=false;slider.minValue=0;slider.maxValue=1;
            var fill=Rect("Fill",bar,0,0,590,16);fill.gameObject.AddComponent<Image>().color=new Color(1,.77f,.35f);slider.fillRect=fill;fill.offsetMin=fill.offsetMax=Vector2.zero;Set(lighthouse,"qteProgress",slider);
        }
        switch(name) {
        case "Beach":
            player.transform.position=new Vector3(-2.5f,-2.2f,-1);
            Door("House","Casa",new Vector3(5,2.4f,-1),new Vector3(5,-.6f,-1),new Vector3(3,3,.4f));break;
        case "House":
            Door("Island","Centro",new Vector3(-7,-.5f,-1),new Vector3(-6.4f,-1.5f,-1),new Vector3(2,3,.4f));break;
        case "Island":
            Door("Beach","Praia",new Vector3(-3.5f,-3.8f,-1),new Vector3(-3.5f,-3.5f,-1),new Vector3(2,1,.4f));
            Door("House","Casa",new Vector3(-6,2,-1),new Vector3(-5.3f,-.6f,-1),new Vector3(3,3,.4f));
            Door("Cave","Caverna",new Vector3(6.6f,1.4f,-1),new Vector3(6,-.6f,-1),new Vector3(3,3,.4f));
            Door("Lighthouse","Farol",new Vector3(3,2.4f,-1),new Vector3(3,-.6f,-1),new Vector3(2,2,.4f));break;
        case "Cave": Door("Island","Centro",new Vector3(-7,.8f,-1),new Vector3(-6.5f,-1.5f,-1),new Vector3(2,3,.4f));break;
        case "Lighthouse": Door("Island","Centro",new Vector3(-6,-2,-1),new Vector3(-5,-2.6f,-1),new Vector3(3,3,.4f));break;
        }
        Set(location,"hover",hover.gameObject);
        var focus=new[]{Ref(scroll,"scrollPanel"),Ref(scroll,"returnPanel"),Ref(miningUI,"miningPanel"),Ref(lighthouse,"panel"),Ref(inventory,"panel"),Named("DialoguePanel"),Ref(Object.FindAnyObjectByType<PauseController>(),"pausePanel"),Ref(Object.FindAnyObjectByType<SettingsMenuUI>(),"settingsPanel")};
        var focusData=new SerializedObject(location);var focusArray=focusData.FindProperty("focusPanels");focusArray.arraySize=focus.Length;for(int f=0;f<focus.Length;f++)focusArray.GetArrayElementAtIndex(f).objectReferenceValue=focus[f];focusData.ApplyModifiedPropertiesWithoutUndo();
        // Chegadas em área livre, sem reaparecer sobre o clique de uma saída.
        loc=new SerializedObject(location);arrivals=loc.FindProperty("arrivals");arrivals.arraySize=5;
        for(int a=0;a<5;a++){var entry=arrivals.GetArrayElementAtIndex(a);entry.FindPropertyRelative("from").stringValue=scenes[a];entry.FindPropertyRelative("position").vector3Value=new Vector3(0,-2.7f,-1);}loc.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+name+".unity");
    }
    static void BuildEnding()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("EndingCanvas",typeof(Canvas),typeof(CanvasScaler));root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);
        var image=Rect("Illustration",root.transform,0,0,1280,720).gameObject.AddComponent<RawImage>();
        var beam=Rect("Light",root.transform,0,190,1280,350).gameObject.AddComponent<BeaconBeamGraphic>();beam.color=new Color(1,.85f,.45f,0);
        var band=Rect("CaptionBackground",root.transform,0,-265,1280,140);band.gameObject.AddComponent<Image>().color=new Color(.02f,.04f,.06f,.9f);
        var caption=Text("Caption",band,"",0,0,1120,120,28);
        Text("SkipHint",root.transform,"Esc  •  Ir aos créditos",470,320,300,35,18);
        var shade=Rect("Fade",root.transform,0,0,1280,720);shade.gameObject.AddComponent<Image>().color=Color.black;var group=shade.gameObject.AddComponent<CanvasGroup>();
        var cinematic=root.AddComponent<EndingCutscene>();Set(cinematic,"picture",image);Set(cinematic,"caption",caption);Set(cinematic,"shade",group);Set(cinematic,"lightBeam",beam);
        var data=new SerializedObject(cinematic);var shots=data.FindProperty("shots");shots.arraySize=3;string[] names={"05-farol","03-centro","01-praia"};for(int i=0;i<3;i++)shots.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/Backgrounds/Story/"+names[i]+".png");data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Ending.unity");
    }
}
