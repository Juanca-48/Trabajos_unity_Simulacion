using UnityEngine;

public class Obstaculos : MonoBehaviour, IObjetoColisionable
{
    public enum TipoObstaculo
    {
        Normal,
        SuperRebote
    }
    
    public enum FormaObstaculo
    {
        Circular,
        Rectangular
    }
    
    [Header("Propiedades")]
    public TipoObstaculo tipo = TipoObstaculo.Normal;
    public FormaObstaculo forma = FormaObstaculo.Circular;
    
    [Header("Dimensiones Circulares")]
    [SerializeField] private float radio = 1f;
    
    [Header("Dimensiones Rectangulares")]
    [SerializeField] private Vector2 tamanoRectangulo = new Vector2(2f, 1f);
    
    [Header("Visualización")]
    [SerializeField] private bool mostrarGizmosEnEditor = true;
    [SerializeField] private Color colorGizmoNormal = Color.yellow;
    [SerializeField] private Color colorGizmoSuperRebote = Color.red;
    
    // Factor de rebote para obstáculos con súper rebote
    private const float SuperRebote = 5.0f;
    
    // Variable estática para manejar todos los obstáculos
    private static bool sistemaInicializado = false;
    
    // Propiedades públicas para acceder a las dimensiones
    public float Radio => radio;
    public Vector2 TamanoRectangulo => tamanoRectangulo;
    public FormaObstaculo Forma => forma;
    
    void Awake()
    {
        // Asegurarnos de que tiene el tag correcto
        if (gameObject.tag != "Obstaculo")
        {
            gameObject.tag = "Obstaculo";
            Debug.Log($"[Obstaculo] Tag cambiado a 'Obstaculo' para {gameObject.name}");
        }
        
        // Calcular dimensiones basado en la escala del objeto
        CalcularDimensiones();
    }
    
    void Start()
    {
        // Registrar el obstáculo en el sistema de física
        RegistrarEnSistemaFisica();
        
        // En el primer obstáculo que se inicia, buscar otros obstáculos en la escena
        if (!sistemaInicializado)
        {
            sistemaInicializado = true;
            BuscarYRegistrarTodosLosObstaculos();
        }
    }
    
    private void CalcularDimensiones()
    {
        if (GetComponent<SpriteRenderer>() != null)
        {
            // Para objetos con sprite, usar el tamaño del sprite
            Vector2 tamanoSprite = GetComponent<SpriteRenderer>().bounds.size;
            
            if (forma == FormaObstaculo.Circular)
            {
                radio = Mathf.Max(tamanoSprite.x, tamanoSprite.y) / 2f;
            }
            else
            {
                tamanoRectangulo = tamanoSprite;
            }
        }
        else
        {
            // Para otros objetos, usar la escala
            if (forma == FormaObstaculo.Circular)
            {
                radio = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
            }
            else
            {
                tamanoRectangulo = new Vector2(transform.localScale.x, transform.localScale.y);
            }
        }
    }
    
    private void RegistrarEnSistemaFisica()
    {
        if (SistemaFisica.instancia != null)
        {
            // Masa alta para que sean prácticamente inamovibles
            float masaEstatica = 1000f;
            
            if (forma == FormaObstaculo.Circular)
            {
                // Usar el método original para objetos circulares
                SistemaFisica.instancia.RegistrarObjeto(gameObject, masaEstatica, radio, Vector2.zero);
                Debug.Log($"[Obstaculo] {gameObject.name} registrado como circular. Radio: {radio}");
            }
            else
            {
                // Usar el nuevo método específico para objetos rectangulares
                SistemaFisica.instancia.RegistrarObjetoRectangular(gameObject, masaEstatica, tamanoRectangulo, Vector2.zero);
                Debug.Log($"[Obstaculo] {gameObject.name} registrado como rectangular. Tamaño: {tamanoRectangulo}");
            }
        }
        else
        {
            Debug.LogError("[Obstaculo] No se encontró el SistemaFisica en la escena");
        }
    }
    
    private void BuscarYRegistrarTodosLosObstaculos()
    {
        // Buscar todos los obstáculos que no tengan colisiones registradas
        Obstaculos[] todosLosObstaculos = FindObjectsOfType<Obstaculos>();
        
        foreach (Obstaculos otroObstaculo in todosLosObstaculos)
        {
            // Asegurarse de que todos los obstáculos estén registrados
            if (otroObstaculo != this && !otroObstaculo.gameObject.CompareTag("Obstaculo"))
            {
                otroObstaculo.gameObject.tag = "Obstaculo";
            }
        }
        
        Debug.Log($"[Obstaculo] Sistema inicializado. Se verificaron {todosLosObstaculos.Length} obstáculos en la escena");
    }
    
    // Método simplificado que delega la verificación al SistemaFisica
    public bool VerificarColision(Vector2 posicionObjeto, float radioObjeto)
    {
        // Este método se mantiene para compatibilidad, pero ahora el SistemaFisica
        // maneja automáticamente las colisiones según la forma del objeto
        Vector2 posicionObstaculo = transform.position;
        
        if (forma == FormaObstaculo.Circular)
        {
            // Colisión circular tradicional
            float distancia = Vector2.Distance(posicionObjeto, posicionObstaculo);
            return distancia <= (radio + radioObjeto);
        }
        else
        {
            // Colisión rectángulo vs círculo usando la lógica mejorada
            return ColisionRectanguloCirculoMejorada(posicionObjeto, radioObjeto, posicionObstaculo, tamanoRectangulo);
        }
    }
    
    private bool ColisionRectanguloCirculoMejorada(Vector2 posCirculo, float radioCirculo, Vector2 posRectangulo, Vector2 tamanoRect)
    {
        // Usar la misma lógica que el SistemaFisica para consistencia
        Vector2 mitadTamano = tamanoRect / 2f;
        Vector2 minRect = posRectangulo - mitadTamano;
        Vector2 maxRect = posRectangulo + mitadTamano;
        
        // Encontrar el punto más cercano del rectángulo al círculo
        Vector2 puntoMasCercano = new Vector2(
            Mathf.Clamp(posCirculo.x, minRect.x, maxRect.x),
            Mathf.Clamp(posCirculo.y, minRect.y, maxRect.y)
        );
        
        // Verificar si la distancia es menor al radio
        float distancia = Vector2.Distance(posCirculo, puntoMasCercano);
        return distancia <= radioCirculo;
    }
    
    public void OnColision(GameObject otroObjeto)
    {
        if (otroObjeto == null)
        {
            // Colisión con los límites
            Debug.Log($"[Obstaculo] {gameObject.name} colisionó con los límites del área");
            return;
        }
        
        // Verificar con qué colisionó
        if (otroObjeto.CompareTag("Bola"))
        {
            Debug.Log($"[Obstaculo] {gameObject.name} ({forma}) golpeado por bola");
            
            // Si es un súper rebote, aplicar fuerza extra a la bola
            if (tipo == TipoObstaculo.SuperRebote)
            {
                Vector2 direccionRebote = CalcularDireccionReboteOptimizada(otroObjeto.transform.position);
                SistemaFisica.instancia.AplicarFuerza(otroObjeto, direccionRebote * SuperRebote);
                
                Debug.Log($"[Obstaculo] Súper rebote aplicado a {otroObjeto.name} con dirección {direccionRebote}");
            }
        }
        else if (otroObjeto.CompareTag("Proyectil"))
        {
            Debug.Log($"[Obstaculo] {gameObject.name} ({forma}) golpeado por proyectil");
            
            // Los proyectiles también pueden activar súper rebote
            if (tipo == TipoObstaculo.SuperRebote)
            {
                Vector2 direccionRebote = CalcularDireccionReboteOptimizada(otroObjeto.transform.position);
                SistemaFisica.instancia.AplicarFuerza(otroObjeto, direccionRebote * SuperRebote * 0.7f); // Menos fuerza para proyectiles
                
                Debug.Log($"[Obstaculo] Súper rebote aplicado a proyectil {otroObjeto.name}");
            }
        }
    }
    
    private Vector2 CalcularDireccionReboteOptimizada(Vector3 posicionObjeto)
    {
        Vector2 posObstaculo = transform.position;
        Vector2 posObjeto = posicionObjeto;
        
        if (forma == FormaObstaculo.Circular)
        {
            // Para círculos, dirección simple desde el centro
            return (posObjeto - posObstaculo).normalized;
        }
        else
        {
            // Para rectángulos, usar lógica mejorada que determina el lado de impacto
            return CalcularDireccionReboteRectangularMejorada(posObjeto, posObstaculo);
        }
    }
    
    private Vector2 CalcularDireccionReboteRectangularMejorada(Vector2 posObjeto, Vector2 posObstaculo)
    {
        Vector2 mitadTamano = tamanoRectangulo / 2f;
        Vector2 diferencia = posObjeto - posObstaculo;
        
        // Calcular qué lado del rectángulo está más cerca
        Vector2 diferenciaNormalizada = new Vector2(
            diferencia.x / mitadTamano.x,
            diferencia.y / mitadTamano.y
        );
        
        Vector2 direccionRebote;
        
        if (Mathf.Abs(diferenciaNormalizada.x) > Mathf.Abs(diferenciaNormalizada.y))
        {
            // Colisión en lado izquierdo o derecho
            direccionRebote = new Vector2(Mathf.Sign(diferencia.x), diferencia.y * 0.3f).normalized;
        }
        else
        {
            // Colisión en lado superior o inferior
            direccionRebote = new Vector2(diferencia.x * 0.3f, Mathf.Sign(diferencia.y)).normalized;
        }
        
        // Si la dirección calculada es cero, usar dirección desde el centro
        if (direccionRebote.magnitude < 0.1f)
        {
            direccionRebote = diferencia.normalized;
        }
        
        return direccionRebote;
    }
    
    void OnDestroy()
    {
        // Eliminar del sistema de física cuando se destruye
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
        }
    }
    
    // Gizmos que se muestran siempre en el editor
    void OnDrawGizmos()
    {
        if (!mostrarGizmosEnEditor) return;
        
        // Color basado en el tipo de obstáculo
        Color colorGizmo = tipo == TipoObstaculo.SuperRebote ? colorGizmoSuperRebote : colorGizmoNormal;
        colorGizmo.a = 0.3f; // Transparencia para que no tape completamente el objeto
        
        Gizmos.color = colorGizmo;
        
        if (forma == FormaObstaculo.Circular)
        {
            DibujarGizmosCircular(colorGizmo);
        }
        else
        {
            DibujarGizmosRectangular(colorGizmo);
        }
    }
    
    private void DibujarGizmosCircular(Color color)
    {
        float radioActual = radio;
        if (radioActual <= 0)
        {
            // Calcular radio temporalmente si no está definido
            if (GetComponent<SpriteRenderer>() != null)
            {
                Vector2 tamanoSprite = GetComponent<SpriteRenderer>().bounds.size;
                radioActual = Mathf.Max(tamanoSprite.x, tamanoSprite.y) / 2f;
            }
            else
            {
                radioActual = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
            }
        }
        
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radioActual);
        
        // Dibujar el borde más visible
        color.a = 0.8f;
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radioActual);
    }
    
    private void DibujarGizmosRectangular(Color color)
    {
        Vector2 tamanoActual = tamanoRectangulo;
        if (tamanoActual == Vector2.zero)
        {
            // Calcular tamaño temporalmente si no está definido
            if (GetComponent<SpriteRenderer>() != null)
            {
                tamanoActual = GetComponent<SpriteRenderer>().bounds.size;
            }
            else
            {
                tamanoActual = new Vector2(transform.localScale.x, transform.localScale.y);
            }
        }
        
        // Dibujar cubo relleno (semitransparente)
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(tamanoActual.x, tamanoActual.y, 0.1f));
        
        // Dibujar el borde más visible
        color.a = 0.8f;
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, new Vector3(tamanoActual.x, tamanoActual.y, 0.1f));
        
        // Dibujar líneas adicionales para mostrar los ejes principales (útil para depuración)
        if (tipo == TipoObstaculo.SuperRebote)
        {
            Gizmos.color = Color.white;
            Vector3 pos = transform.position;
            Vector2 mitad = tamanoActual / 2f;
            
            // Líneas horizontales y verticales para mostrar los ejes de rebote
            Gizmos.DrawLine(pos + new Vector3(-mitad.x, 0, 0), pos + new Vector3(mitad.x, 0, 0));
            Gizmos.DrawLine(pos + new Vector3(0, -mitad.y, 0), pos + new Vector3(0, mitad.y, 0));
        }
    }
    
    // Gizmos que se muestran solo cuando el objeto está seleccionado
    void OnDrawGizmosSelected()
    {
        // Mostrar información adicional cuando está seleccionado
        Color colorSeleccionado = Color.white;
        Gizmos.color = colorSeleccionado;
        
        if (forma == FormaObstaculo.Circular)
        {
            float radioActual = radio > 0 ? radio : Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
            
            // Dibujar círculos múltiples para mejor visibilidad
            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawWireSphere(transform.position, radioActual + (i * 0.05f));
            }
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = colorSeleccionado;
            UnityEditor.Handles.Label(transform.position + Vector3.up * (radioActual + 0.5f), 
                $"{gameObject.name}\nTipo: {tipo}\nForma: {forma}\nRadio: {radioActual:F2}");
            #endif
        }
        else
        {
            Vector2 tamanoActual = tamanoRectangulo != Vector2.zero ? tamanoRectangulo : new Vector2(transform.localScale.x, transform.localScale.y);
            
            // Dibujar rectángulos múltiples para mejor visibilidad
            for (int i = 0; i < 3; i++)
            {
                Vector3 tamanoConBorde = new Vector3(tamanoActual.x + (i * 0.1f), tamanoActual.y + (i * 0.1f), 0.1f);
                Gizmos.DrawWireCube(transform.position, tamanoConBorde);
            }
            
            // Mostrar puntos de las esquinas para mejor comprensión del área
            Vector2 mitad = tamanoActual / 2f;
            Vector3 pos = transform.position;
            Gizmos.color = Color.cyan;
            float tamañoPunto = 0.1f;
            
            Gizmos.DrawSphere(pos + new Vector3(-mitad.x, -mitad.y, 0), tamañoPunto);
            Gizmos.DrawSphere(pos + new Vector3(mitad.x, -mitad.y, 0), tamañoPunto);
            Gizmos.DrawSphere(pos + new Vector3(-mitad.x, mitad.y, 0), tamañoPunto);
            Gizmos.DrawSphere(pos + new Vector3(mitad.x, mitad.y, 0), tamañoPunto);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = colorSeleccionado;
            UnityEditor.Handles.Label(transform.position + Vector3.up * (tamanoActual.y/2f + 0.5f), 
                $"{gameObject.name}\nTipo: {tipo}\nForma: {forma}\nTamaño: {tamanoActual.x:F1} x {tamanoActual.y:F1}");
            #endif
        }
    }
    
    // Método de utilidad para validar configuración en el editor
    void OnValidate()
    {
        // Asegurar valores mínimos
        if (radio <= 0) radio = 0.5f;
        if (tamanoRectangulo.x <= 0) tamanoRectangulo.x = 1f;
        if (tamanoRectangulo.y <= 0) tamanoRectangulo.y = 1f;
        
        // Recalcular dimensiones si cambian en el editor
        if (Application.isPlaying)
        {
            CalcularDimensiones();
        }
    }
    
    // Métodos de utilidad para debugging y ajustes dinámicos
    public void CambiarTipo(TipoObstaculo nuevoTipo)
    {
        tipo = nuevoTipo;
        Debug.Log($"[Obstaculo] {gameObject.name} cambió a tipo {nuevoTipo}");
    }
    
    public void CambiarForma(FormaObstaculo nuevaForma)
    {
        forma = nuevaForma;
        CalcularDimensiones();
        
        // Re-registrar en el sistema de física con la nueva forma
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
            RegistrarEnSistemaFisica();
        }
        
        Debug.Log($"[Obstaculo] {gameObject.name} cambió a forma {nuevaForma}");
    }
    
    public void AjustarTamaño(float nuevoRadio)
    {
        if (forma == FormaObstaculo.Circular)
        {
            radio = nuevoRadio;
            Debug.Log($"[Obstaculo] {gameObject.name} radio ajustado a {nuevoRadio}");
        }
    }
    
    public void AjustarTamaño(Vector2 nuevoTamano)
    {
        if (forma == FormaObstaculo.Rectangular)
        {
            tamanoRectangulo = nuevoTamano;
            Debug.Log($"[Obstaculo] {gameObject.name} tamaño ajustado a {nuevoTamano}");
        }
    }
}