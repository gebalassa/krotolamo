using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArenaController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] GameObject backgroundContainer;
    [SerializeField] List<BackgroundElements> backgroundPrefabs;
    List<BackgroundElements> backgroundPool = new List<BackgroundElements>();
    BackgroundElements currentBackground;

    [Header("Crowd")]
    [SerializeField] List<CrowdNPC> crowd;
    [SerializeField] CrowdNPC[] backFrontCrowd;
    [Tooltip("Distancia entre los espectadores")]
    [SerializeField] int maxDistance = 1;
    [Tooltip("Indica la cantidad minima de espectadores")]
    [SerializeField] int minActives = 7;

    List<Rigidbody2D> crowdRigidBoy = new List<Rigidbody2D>();

    [Header("Jump")]
    [SerializeField] [Range(0f, 5f)] float punchMinForce = 1.5f;
    [SerializeField] [Range(0f, 5f)] float punchMaxForce = 2.5f;

    [Header("Support Crowd")]
    [SerializeField] List<GameObject> supportCrowdPrefab;
    [SerializeField] GameObject supportCrowdParent;
    [SerializeField] float supportCrowdSmallY = -0.23f;
    [SerializeField] float supportCrowdMediumY = 0.18f;
    [SerializeField] float supportCrowdBigY = 0.27f;
    [SerializeField] bool supporNPCWithOutCharacter = true;
    List<SupportCrowd> supportCrownObjects = new List<SupportCrowd>();
    List<JankenUp.NPC.Identifiers> supportCrowdIdentifiers = new List<JankenUp.NPC.Identifiers>();

    [Header("Special Crowd")]
    [SerializeField] List<CrowdNPC> specialCrowdPrefab;
    [SerializeField] Transform specialCrowdParent;
    List<CrowdNPC> specialCrowdInstances = new List<CrowdNPC>();
    List<string> specialNPCNotAllowed = new List<string>();
    bool specialNPCNotAllowedAnyone = false;

    [Header("Camera Inputs")]
    [SerializeField] private LayerMask inputLayerMask;

    // Para desordenar los soportes
    int maxPositionFactor = 8;

    // Llave para determinar si fondo debe ser fijo
    JankenUp.BackgroundElementsIdentifier fixedBackgroundIdentifier = JankenUp.BackgroundElementsIdentifier.Classic;
    int fixedBackgroundColor = -1;
    bool backgroundIsFixed = false;

    // Color aplicado al publico
    Color crowdColor = Color.white;

    // Flags para indicar estado de operaciones
    bool isConfiguring = false;
    bool isShuffleCrowd = false;

    // Start is called before the first frame update
    void Start()
    {
        // Configurar mascara de eventos para camara
        Camera.main.eventMask = inputLayerMask;

        // Configurar los NPC especiales del jugador
        SetupSpecialNPC();

        // Agregar los RigidBody del publico
        foreach(CrowdNPC crowdElement in crowd)
        {
            crowdRigidBoy.Add(crowdElement.GetComponent<Rigidbody2D>());
        }
        foreach(CrowdNPC crowdElement in backFrontCrowd)
        {
            crowdRigidBoy.Add(crowdElement.GetComponent<Rigidbody2D>());
        }

        Config();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Configuración de la Arena
    public void Config()
    {
        if (isConfiguring) return;
        isConfiguring = true;
        ChangeBackground();
        ShuffleCrowd();
        if(supporNPCWithOutCharacter) ConfigureSupportCrowd();
        isConfiguring = false;
    }

    /// <summary>
    /// Se establece un background fijo por defecto
    /// </summary>
    /// <param name="arenaKey"></param>
    public void SetFixedBackground(string arenaKey)
    {
        // Comprobar la existencia de la llave solicitada
        if (!JankenUp.Arenas.identifiers.ContainsKey(arenaKey)) return;
        fixedBackgroundIdentifier = JankenUp.Arenas.identifiers[arenaKey];
        backgroundIsFixed = true;
    }

    /// <summary>
    /// Se establece un color background fijo por defecto
    /// </summary>
    /// <param name="arenaIndex"></param>
    public void SetFixedBackgroundColor(int arenaIndex)
    {
        // Comprobar que no supere el maximo de fondos disponibles
        if (arenaIndex >= JankenUp.Arenas.backgroundLenght) return;
        fixedBackgroundColor = arenaIndex;
    }

    /// <summary>
    /// Cambio de fondo
    /// </summary>
    private void ChangeBackground() {

        // Tomar en cuenta todos los prefabs existentes
        BackgroundElements newBackground = null;

        // Existe un background fijo?
        if (backgroundIsFixed)
        {
            // Revisar si ya fue instanciado, de lo contrario, instanciar
            newBackground = backgroundPool.Find(background => background.GetIdentifier() == fixedBackgroundIdentifier);
            if (!newBackground)
            {
                BackgroundElements prefab = backgroundPrefabs.Find(background => background.GetIdentifier() == fixedBackgroundIdentifier);
                if (prefab)
                {
                    BackgroundElements instantiateBackground = Instantiate(prefab, backgroundContainer.transform).GetComponent<BackgroundElements>();
                    backgroundPool.Add(instantiateBackground);
                    newBackground = instantiateBackground;
                    backgroundPrefabs.Remove(prefab);
                }
            }
        }

        if(!newBackground)
        {
            // Instanciar uno de los background disponibles
            if (backgroundPrefabs.Count > 0)
            {
                // Instanciar uno de los elementos que no se encuentre ya en el listado
                int randomPrefabIndex = Random.Range(0, backgroundPrefabs.Count);
                BackgroundElements instantiateBackground = Instantiate(backgroundPrefabs[randomPrefabIndex], backgroundContainer.transform).GetComponent<BackgroundElements>();
                backgroundPool.Add(instantiateBackground);

                // Eliminar el elemento de los prefabs
                backgroundPrefabs.RemoveAt(randomPrefabIndex);
            }

            // Seleccionar un background al azar
            int randomBackgroundIndex = Random.Range(0, backgroundPool.Count);
            newBackground = backgroundPool[randomBackgroundIndex];
        }

        // Ocultar todos los otros backgrounds
        foreach(BackgroundElements child in backgroundPool)
        {
            if (child != newBackground) child.SetActive(false);
        }

        // Activar el background nuevo
        newBackground.SetActive(true);
        currentBackground = newBackground;

        // Obtener el nuevo color para el publico
        ChangeCrowdColor(currentBackground.GetCrowdColor());

        // Configurar el background
        currentBackground.Config(fixedBackgroundColor);
    }

    // Desordena la audiencia
    private void ShuffleCrowd()
    {
        if (isShuffleCrowd) return;
        isShuffleCrowd = true;
        int actives = Random.Range(minActives, crowd.Count);

        // Desordenar los elementos del crowd
        CrowdNPC temp;
        for (int i = 0; i < crowd.Count; i++)
        {
            int rnd = Random.Range(0, crowd.Count);
            temp = crowd[rnd];
            crowd[rnd] = crowd[i];
            crowd[i] = temp;
        }

        // Bases para realizar la configuración. Pivot entrega la primera ubicación
        // factor indica el multiplo para la nueva posición y actives indica la cantidad a mostrar
        int pivot = 2;
        int factor = 1;

        for( int index = 0; index < crowd.Count; index++)
        {
            // En caso de que el NPC sea de los especiales, comprobar que no este en el listado de los no permitidos
            bool notAllowedThisTime = specialCrowdInstances.Contains(crowd[index])? (specialNPCNotAllowedAnyone || specialNPCNotAllowed.Contains(crowd[index].GetIdentifier())) : false;

            // Indicar si el elemento esta activo o no
            bool active = notAllowedThisTime ? false : actives-- > 0;
            crowd[index].gameObject.SetActive(active);

            // Dirección indica si esta a la derecha o izquierda de los pivotes
            int direction = index % 2 == 0 ? 1 : -1;
            int distance = Random.Range(1, maxDistance + 1) * direction;

            int newX = pivot * direction
                + (distance * factor );

            GameObject c = crowd[index].gameObject;
            c.transform.position = new Vector3(
                newX,
                c.transform.position.y,
                c.transform.position.z);

            if (index != 0 && index % 2 == 0) factor++;

            // Cambiar el color del personaje si es que esta activo
            if (c.activeSelf) crowd[index].ChangeColor(crowdColor);
        }

        // Cambiar el color del publico general
        foreach(CrowdNPC generalCrowd in backFrontCrowd)
        {
            generalCrowd.ChangeColor(crowdColor);
        }

        // Indicar que ya se hizo el shuffle
        isShuffleCrowd = false;
    }

    /*
     * Revisar soportes, crear, activar/desactivar para dar mas variedad, desordenar
     * @return {void}
     * **/
    private void ConfigureSupportCrowd()
    {
        // Revisar que soportes no estan creados
        List<GameObject> supportNotInScene = supportCrowdPrefab.FindAll(prefab => !supportCrowdIdentifiers.Contains(prefab.GetComponent<SupportCrowd>().GetIdentifier()));

        // De manera random, crear algunos de ellos
        if( Random.Range(0,2) == 1)
        {
            foreach(GameObject prefab in supportNotInScene)
            {
                if(Random.Range(0, 2) == 1) CreateSupportNPC(prefab.GetComponent<SupportCrowd>().GetIdentifier(), null);
            }
        }

        // Desordenar y Activar/Desactivar los soportes (Tener en cuenta el tema de los ligados)
        int pivot = 2;
        foreach(SupportCrowd supportCrowd in supportCrownObjects)
        {
            if (!supportCrowd) continue;

            // Dirección indica si esta a la derecha o izquierda de los pivotes
            int direction = Random.Range(0, 2) == 0 ? 1 : -1;
            int distance = Random.Range(1, maxDistance + 1) * direction;

            int newX = pivot * direction
                + (distance * Random.Range(1, maxPositionFactor));

            supportCrowd.transform.position = new Vector3(
                newX,
                supportCrowd.transform.position.y,
                supportCrowd.transform.position.z);

            // Activar/Desactivar
            bool active = supportCrowd.IsLinkedToCharacter() ? true : Random.Range(0, 2) == 1;
            supportCrowd.gameObject.SetActive( active );

            // Cambiar el color del personaje si es que esta activo
            if (active) supportCrowd.ChangeColor(crowdColor);
        }
    }

    // Hacer saltar a la multitud
    public void Jump(int multiplier)
    {

        // Aplicar con distinta fuerza a los distintos personajes del publico
        foreach(Rigidbody2D npc in crowdRigidBoy)
        {
            if(npc && npc.bodyType != RigidbodyType2D.Kinematic)
            {
                float force = Random.Range(punchMinForce * multiplier, punchMaxForce * multiplier);
                npc.velocity = new Vector2(0, force);
            }
        }

        // Indicar al controlador de background de que los elementos en terreno deben saltar (Si opcion esta habilitada)
        if (currentBackground) currentBackground.Jump(punchMinForce, punchMaxForce, multiplier);

    }

    /// <summary>
    /// Comprobacion de existe de un crowd NPC de soporte. En caso de no existir, debe crearse
    /// </summary>
    /// <param name="supportNPCIdentifier">NPC del cual debe comprobarse su existencia ( Y si esta activo )</param>
    /// <param name="character">Controlador al que sera asociado</param>
    public void CheckForSupportNPC( JankenUp.NPC.Identifiers supportNPCIdentifier, CharacterInGameController character ) {

        // Obtener todos los NPC que coincidan con el identificador y que puedan ser ligados
        List<SupportCrowd> filter = supportCrownObjects.FindAll(npc => npc.GetIdentifier() == supportNPCIdentifier);

        if(filter.Count > 0)
        {
            bool linked = false;

            // Comprobar si existe alguno que no este ligado al character target
            foreach(SupportCrowd target in filter)
            {
                linked = target.SetLinkToCharacter(character.gameObject);
                if (linked) break;
            }

            // De no estar ligado a ninguno de los disponibles, crear uno nuevo
            if (!linked) CreateSupportNPC(supportNPCIdentifier, character);
        }
        else
        {
            // Al no existir, crear directamente un nuevo apoyo
            CreateSupportNPC(supportNPCIdentifier, character);
        }


    }

    private void CreateSupportNPC(JankenUp.NPC.Identifiers supportNPCIdentifier, CharacterInGameController character )
    {
        // Crear directamente a partir de los prefabs
        GameObject supportBase = supportCrowdPrefab.Find(prefab => prefab.GetComponent<SupportCrowd>().GetIdentifier() == supportNPCIdentifier);
        if (supportBase)
        {
            float yPos = 0;
            switch (supportBase.GetComponent<SupportCrowd>().GetSize())
            {
                case JankenUp.NPC.Size.Small:
                    yPos = supportCrowdSmallY;
                    break;
                case JankenUp.NPC.Size.Medium:
                    yPos = supportCrowdMediumY;
                    break;
                case JankenUp.NPC.Size.Big:
                    yPos = supportCrowdBigY;
                    break;
            }

            // Cambiar posicion en el eje X
            int pivot = 2;
            int factor = 1;
            int direction = Random.Range(0,2) == 0? 1 : -1;
            int distance = Random.Range(1, maxDistance + 1) * direction;

            int newX = pivot * direction
                + (distance * factor);

            GameObject supportNew = Instantiate(supportBase, new Vector2(newX, yPos), Quaternion.identity);
            supportNew.transform.parent = supportCrowdParent.transform;

            SupportCrowd supportNewSupportCrowd = supportNew.GetComponent<SupportCrowd>();
            supportNewSupportCrowd.SetLinkToCharacter(character? character.gameObject : null);
            supportCrownObjects.Add(supportNewSupportCrowd);
            supportCrowdIdentifiers.Add(supportNewSupportCrowd.GetIdentifier());

            // Agregar el Rigidboy para saltar cuando sea necesario
            crowdRigidBoy.Add(supportNew.GetComponent<Rigidbody2D>());

            // Cambiar el color por el del escenario
            supportNewSupportCrowd.ChangeColor(crowdColor);
        }
    }
    
    /// <summary>
    /// Deshabilita los links entre los soportes actuales
    /// </summary>
    public void ForceUnlinkSupportNPC()
    {
        foreach (SupportCrowd supportCrowd in supportCrownObjects)
        {
            supportCrowd.ForceUnlink();
        }
    }

    /// <summary>
    /// Aplicar el color a cada uno de los elementos del publico actual y tener en cuenta para la creacion de los proximos
    /// </summary>
    /// <param name="newColor"></param>
    public void ChangeCrowdColor(Color newColor) {
        crowdColor = newColor;
    }

    /// <summary>
    /// Indica la lista de personajes de fondo que no pueden ser usados
    /// </summary>
    /// <param name="notAllowed"></param>
    public void SetNotAllowedSpecialNPC(List<string> notAllowed = null, bool noAllowedAnyone = false) {
        specialNPCNotAllowed.Clear();
        if(notAllowed != null) specialNPCNotAllowed.AddRange(notAllowed);
        specialNPCNotAllowedAnyone = noAllowedAnyone;
    }

    /// <summary>
    /// Incluir los personajes de publicos comprados por el jugador
    /// </summary>
    private void SetupSpecialNPC()
    {
        if (specialCrowdPrefab == null || specialCrowdPrefab.Count == 0) return;

        // Obtener los NPC del jugador
        List<string> npcInAccount = GameController.GetCrowds();

        foreach(string npcIdentifier in npcInAccount)
        {
            CrowdNPC npc = specialCrowdPrefab.Find(candidate => candidate.GetIdentifier() == npcIdentifier);
            if (npc)
            {
                CrowdNPC newNPC = Instantiate(npc, specialCrowdParent).GetComponent<CrowdNPC>();
                specialCrowdInstances.Add(newNPC);
                crowd.Add(newNPC);

                // Iniciar en modo desactivado
                newNPC.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Oculta a los NPC que se encuentran en combate si es que estan de publico
    /// </summary>
    /// <param name="npcIdentifiers"></param>
    public void HideNPCInMatch(List<string> npcIdentifiers) {
        foreach(CrowdNPC npc in crowd)
        {
            if (npcIdentifiers.Contains(npc.GetIdentifier())) npc.Hide();
        }
    }
}
