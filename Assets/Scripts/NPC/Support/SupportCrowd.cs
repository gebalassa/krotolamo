using System.Collections;
using UnityEngine;

public class SupportCrowd : CrowdNPC
{
    [Header("Setup")]
    [SerializeField] protected JankenUp.NPC.Identifiers identifier;
    [SerializeField] protected JankenUp.NPC.Size size;

    [Header("Others")]
    [SerializeField] Vector3 positionPostJump;
    [SerializeField] [Range(0,100)] float jumpInTime = 0.4f;

    [Header("Spawn")]
    [SerializeField] int spawnGravity = 2;
    [SerializeField] int spawnPostGravity = 1;
    [SerializeField] PhysicsMaterial2D spawnMaterial;
    [SerializeField] PhysicsMaterial2D spawnPostMaterial;

    // Componentes
    Animator animator;
    Rigidbody2D supportRigidbody;

    // Others
    GameObject linkToCharacter;
    bool alterRigidBobyOnCollision = true;
    bool rigidBodyAlterPropertiesReady = false;
    float changeAfterSeconds = .2f;
    float changeAfterSecondsCurrent = 0;
    bool readyToJump = false;

    new void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        supportRigidbody = GetComponent<Rigidbody2D>();
        transform.transform.position = new Vector3(transform.position.x, positionPostJump.y, transform.position.z);

    }

    // Update is called once per frame
    void Update()
    {
        // Ver si alterar el rigidBody
        if (rigidBodyAlterPropertiesReady)
        {
            changeAfterSecondsCurrent += Time.deltaTime;
            if (changeAfterSecondsCurrent >= changeAfterSeconds) ChangeRigidBodyProperties();
        }
    }

    /*
     * Obtencion del identificador del NPC de soporte
     * @return {JankenUp.NPC.Identifiers} Identificador del soporte
     * **/
    public JankenUp.NPC.Identifiers GetIdentifier()
    {
        return identifier;
    }

    /**
     * Invokar al soporte para que haga parte del ataque del personaje
     * @return {IEnumerator}
    */
    public virtual IEnumerator CallSupport()
    {
        while (!readyToJump) yield return null;

        // Enviar a volar al PJ
        yield return StartCoroutine(JumpFX());
    }

    /**
     * Sacar fuera de los limites de la pantalla
     * */
    private IEnumerator JumpFX()
    {
        supportRigidbody.bodyType = RigidbodyType2D.Kinematic;

        positionPostJump = new Vector3(transform.position.x, positionPostJump.y, positionPostJump.z);
        float distance = Vector2.Distance(transform.position, positionPostJump);
        float speed = distance / jumpInTime * Time.deltaTime;
        float currentTime = 0f;

        do
        {
            transform.position = Vector2.MoveTowards(transform.position, positionPostJump, speed);
            currentTime += Time.deltaTime;
            yield return null;
        } while (Vector2.Distance(transform.position, positionPostJump) > Mathf.Epsilon && currentTime < jumpInTime);

        transform.position = positionPostJump;
        StopJumpFX();
    }

    /**
     * Detener el salto del personaje
     * */
    private void StopJumpFX()
    {
        supportRigidbody.velocity = new Vector2(0, 0);
    }

    /**
     * Indicar al soporte que debe volver a su posicion original
     * @return {void}
    */
    public virtual void ReturnToPosition()
    {
        supportRigidbody.bodyType = RigidbodyType2D.Dynamic;
        supportRigidbody.sharedMaterial = spawnMaterial;
        supportRigidbody.gravityScale = spawnGravity;
        alterRigidBobyOnCollision = true;
        readyToJump = false;
    }

    /*
     * Al colisionar con la plataforma, ver si en necesario alterar las propiedades
     * @params {Collision2D collision} Collision con la que hace contacto
     * @return {void}
     * **/
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ver si se debe cambiar propiedades de rigidbody
        if (alterRigidBobyOnCollision) rigidBodyAlterPropertiesReady = true;
    }

    /*
     * Cambiar las propiedades del rigidbody al colisionar
     * @return {void}
     * **/
    protected virtual void ChangeRigidBodyProperties()
    {
        alterRigidBobyOnCollision = false;
        rigidBodyAlterPropertiesReady = false;
        changeAfterSecondsCurrent = 0;
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.sharedMaterial = spawnPostMaterial;
        rigidBody.gravityScale = spawnPostGravity;
        readyToJump = true;
    }

    /*
     * Obtener el size del soporte
     * @return {JankenUp.NPC.Size}
     * **/
    public JankenUp.NPC.Size GetSize()
    {
        return size;
    }

    /*
     * Establecer el personaje al que estara ligado
     * @param {GameObject} character
     * @return {bool}
     * **/
    public bool SetLinkToCharacter(GameObject character)
    {
        CharacterInGameController chGameControler = linkToCharacter? linkToCharacter.GetComponent<CharacterInGameController>() : null;
        if (linkToCharacter == null || (chGameControler && chGameControler.GetSupportNPC() == null))
        {
            linkToCharacter = character;
            CharacterInGameController characterInGameController = character? character.GetComponent<CharacterInGameController>() : null;
            if(characterInGameController) characterInGameController.SetSupportNPC(this);
            return true;
        }
        else
        {
            return character == linkToCharacter;
        }
    }

    /*
     * Saber si existe un personaje ligado
     * @return {bool}
     * **/
    public bool IsLinkedToCharacter()
    {
        return linkToCharacter != null;
    }

    /// <summary>
    /// Deshabilitacion forzoda del link del personaje
    /// </summary>
    public void ForceUnlink() {
        linkToCharacter = null;
    }
}