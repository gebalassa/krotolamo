using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementSelector : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform container;
    [SerializeField] Transform target;

    [Header("Bank")]
    [SerializeField] GameObject bank;
    [SerializeField] bool bankCharacterPool = false;

    [Header("Prefab")]
    [SerializeField] GameObject elementPrefab;

    [Header("Options")]
    [SerializeField] bool multiple = false;
    [SerializeField] bool showHeader = true;
    [SerializeField] [Range(0, 1)] float adjustContentAfter = 0.25f;
    [SerializeField] float scrollDelta = 0.1f;
    [SerializeField] float scrollTime = 0.1f;

    // Componentes recurrentes
    ElementBank elementorBank;
    GridLayoutGroup layout;

    // Elementos seleccionados
    List<ElementItem> elementsItem = new List<ElementItem>();
    List<ElementItem> elementsSelected = new List<ElementItem>();

    // Delegados para indicar que ha cambiado la seleccion
    public delegate void ItemSelectedDelegate(List<ElementItem> elementItems, bool validChange);
    public ItemSelectedDelegate itemSelectedDelegate;

    // Utiles
    public bool IsReady { get { return _isReady; } }
    private bool _isReady = false;
    private int lastRow = -1;

    // Use this for initialization
    IEnumerator Start()
    {
        Populate();
        yield return StartCoroutine(AdjustContentHeightCoroutine());
        _isReady = true;
    }

    /// <summary>
    /// Llenado del contenido seleccionable
    /// </summary>
    void Populate()
    {
        // Obtener los elementos y crear
        elementorBank = bankCharacterPool? CharacterPool.Instance.GetComponent<ElementBank>() : bank.GetComponent<ElementBank>();
        List<GameObject> elements = elementorBank.GetAll();
        foreach(GameObject element in elements)
        {
            ElementItem item = Instantiate(elementPrefab, target).GetComponent<ElementItem>();
            item.ShowHeader(showHeader);
            item.SetElementSource(element);
            item.onClickDelegate += OnElementClicked;
            elementsItem.Add(item);
        }
    }

    /// <summary>
    /// Realizar verificaciones cuando un elemento ha sido clickeado
    /// </summary>
    /// <param name="elementItem"></param>
    void OnElementClicked(ElementItem elementItem)
    {
        // Scrolear hacia el element
        ScrollToElement(elementItem);

        // Se deben hacer las comprobaciones en el banco de data
        if (elementorBank != null)
        {
            bool valid = elementorBank.CheckItem(elementItem.GetElementSource());
            bool change = Select(elementItem);

            // Llamar a delegado de guardar la informacion si todas las comprobaciones fueron exitosas
            if (change && itemSelectedDelegate != null) itemSelectedDelegate(elementsSelected, valid);
        }
    }

    /// <summary>
    /// Scrollear hacia el elemento indicado
    /// </summary>
    /// <param name="elementItem"></param>
    void ScrollToElement(ElementItem elementItem)
    {
        // Calcular la fila
        bool firstRow = true;
        int columns = 1;
        int rows = 1;
        GetColumnAndRow(out columns, out rows);
        int index = elementsItem.FindIndex(e => e.GetIdentifier() == elementItem.GetIdentifier());
        if (index > columns) firstRow = false;

        // Nota: Ajuste momentaneo para problema de seleccion de fila inferior
        float delta = 0;
        int currentRow = index / columns;
        if ((lastRow != -1 && lastRow < currentRow) || currentRow >= rows - 2) delta = layout.cellSize.y / 2;
        lastRow = currentRow;

        // Calcular ubicacion % del item
        float verticalNormalizedPosition = (float)System.Math.Round(Mathf.Abs((elementItem.transform.localPosition.y - delta) / ((RectTransform)container).rect.height), 1) - (firstRow ? scrollDelta : 0);

        // Mostrar el main
        System.Action<ITween<float>> updateScroll = (t) =>
        {
            if (scrollRect) scrollRect.verticalNormalizedPosition = t.CurrentValue;
        };

        // Mover a ubicacion de personaje
        float toNormalizedPosition = 1 - verticalNormalizedPosition;

        // Hacer fade in y mostrar elemento
        scrollRect.gameObject.Tween(string.Format("Scroll{0}", scrollRect.GetInstanceID()), scrollRect.verticalNormalizedPosition, toNormalizedPosition, scrollTime, TweenScaleFunctions.QuadraticEaseOut, updateScroll);
    }

    /// <summary>
    /// Seleccionar item del listado
    /// </summary>
    /// <param name="elementItem"></param>
    public bool Select(ElementItem elementItem)
    {
        if (multiple)
        {
            if (elementsSelected.Contains(elementItem))
            {
                elementsSelected.Remove(elementItem);
                elementItem.Select(false);
                return true;
            }
            else
            {
                elementItem.Select();
                elementsSelected.Add(elementItem);
                return true;
            }
        }
        else if( !elementsSelected.Contains(elementItem) )
        {
            foreach (ElementItem targetElement in elementsSelected)
            {
                targetElement.Select(false);
            }
            elementsSelected.Clear();
            elementItem.Select();
            elementsSelected.Add(elementItem);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Seleccionar un item en base a su identificador
    /// </summary>
    /// <param name="identifier"></param>
    public void SelectByIdentifier(string identifier)
    {
        ElementItem selected = elementsItem.Find(e => e.GetIdentifier() == identifier);
        if (selected) OnElementClicked(selected);
    }

    /// <summary>
    /// Realiza una actualizacion del contenido en base al identificador del elemento
    /// </summary>
    /// <param name="identifier"></param>
    public void UpdateByIdentifier(string identifier)
    {
        ElementItem selected = elementsItem.Find(e => e.GetIdentifier() == identifier);
        if (selected) selected.Refresh();
    }

    /// <summary>
    /// Realiza el ajuste del ancho el contenedor de data de personajes
    /// </summary>
    /// <returns></returns>
    IEnumerator AdjustContentHeightCoroutine()
    {
        yield return new WaitForSeconds(adjustContentAfter);
        AdjustContentHeight();
    }

    /// <summary>
    /// Realiza el ajuste del alto sobre una transformacion especifica.
    /// </summary>
    private void AdjustContentHeight()
    {
        // Continuar solo si tiene el componente de GridLayout
        if (layout == null) layout = target.GetComponent<GridLayoutGroup>();
        if (layout == null) return;

        // Calcular cuantas filas existen segun el tamano de las celdas
        int columns = 1;
        int rows = 1;
        GetColumnAndRow(out columns, out rows);

        float pageHeight =  layout.padding.top + layout.padding.bottom + ( layout.cellSize.y + layout.spacing.y ) * rows;
        // Actualizar el elementos
        RectTransform rectVerticalLayout = ((RectTransform)container);
        rectVerticalLayout.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pageHeight);
    }

    /// <summary>
    /// Obtencion del numero de columnas y filas del selector
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    private void GetColumnAndRow(out int column, out int row)
    {
        // Gracias a: https://stackoverflow.com/questions/52353898/get-column-and-row-of-count-from-gridlayoutgroup-programmatically
        if (layout == null) layout = target.GetComponent<GridLayoutGroup>();
        column = 0;
        row = 0;

        if (layout == null || layout.transform.childCount == 0) return;

        column = 1;
        row = 1;

        // Get the first child GameObject of the GridLayoutGroup
        RectTransform firstChildObj = layout.transform.
            GetChild(0).GetComponent<RectTransform>();

        Vector2 firstChildPos = firstChildObj.anchoredPosition;
        bool stopCountingColumns = false;

        // Loop through the rest of the child object
        for (int i = 1; i < layout.transform.childCount; i++){
            // Get the next child
            RectTransform currentChildObj = layout.transform.
            GetChild(i).GetComponent<RectTransform>();

            Vector2 currentChildPos = currentChildObj.anchoredPosition;
            if (firstChildPos.x == currentChildPos.x){
                row++;
                stopCountingColumns = true;
            }
            else{
                if (!stopCountingColumns) column++;
            }
        }
    }

    /// <summary>
    /// Realizar el cambio de personaje en base a la accion de un joystick
    /// </summary>
    /// <param name="action"></param>
    public bool MoveTo(JoystickAction action)
    {
        bool canContinue = true;
        if (elementsSelected.Count == 0) return canContinue;

        // Calcular cuantas filas existen segun el tamano de las celdas
        int columns = 1;
        int rows = 1;
        GetColumnAndRow(out columns, out rows);
        int index = elementsItem.FindIndex(e => e.GetIdentifier() == elementsSelected[0].GetIdentifier());
        int currentColumn = index % columns;
        int currentRow = (int) Mathf.Ceil(index / columns);
        switch (action)
        {
            case JoystickAction.Left:
                currentColumn--;
                if (currentColumn < 0)
                {
                    currentColumn = columns - 1;
                    currentRow--;
                }
                index = currentRow * columns + currentColumn;
                index = index < 0 ? elementsItem.Count - 1 : index;
                canContinue = false;
                break;
            case JoystickAction.Right:
                currentColumn++;
                if (currentColumn >= columns)
                {
                    currentColumn = 0;
                    currentRow++;
                }
                index = currentRow * columns + currentColumn;
                index = index >= elementsItem.Count ? 0 : index;
                canContinue = false;
                break;
            case JoystickAction.Up:
                currentRow--;
                if (currentRow < 0) currentRow = rows - 1;
                canContinue = false;
                index = currentRow * columns + currentColumn;
                index = index >= elementsItem.Count ? (currentRow - 1) * columns + currentColumn : index;
                break;
            case JoystickAction.Down:
                currentRow++;
                if (currentRow >= rows) currentRow = 0;
                index = currentRow * columns + currentColumn;
                index = index >= elementsItem.Count ? currentColumn : index;
                break;
        }
        ElementItem newElement = elementsItem[index];
        if (newElement) OnElementClicked(newElement);
        return canContinue;
    }
}