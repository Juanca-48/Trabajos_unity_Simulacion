using UnityEngine;

public class Obstaculos : MonoBehaviour, IObjetoColisionable
{
    public enum TipoObstaculo
    {
        Normal,
        SuperRebote
        Amortiguador,
    }
    
    [Header("Propiedades")]
    public TipoObstaculo tipo = TipoObstaculo.Normal;
    public float radio = 1f;
    
    // Factor de rebote para obstáculos con súper rebote
    private const float SuperRebote = 5.0f;

    private const float Amortiguador = 0.5f;
    
    // Variable estática para manejar todos los obstáculos
    private static bool sistemaInicializado = false;
    
    void Awake()
    {
        // Asegurarnos de que tiene el tag correcto
        if (gameObject.tag != "Obstaculo")
        {
            gameObject.tag = "Obstaculo";
            Debug.Log($"[Obstaculo] Tag cambiado a 'Obstaculo' para {gameObject.name}");
        }
        
        // Calcular radio basado en la escala del objeto
        CalcularRadio();
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
    
    private void CalcularRadio()
    {
        if (GetComponent<SpriteRenderer>() != null)
        {
            // Para objetos con sprite, usar el tamaño del sprite
            Vector2 tamanoSprite = GetComponent<SpriteRenderer>().bounds.size;
            radio = Mathf.Max(tamanoSprite.x, tamanoSprite.y) / 2f;
        }
        else
        {
            // Para otros objetos, usar la escala
            radio = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2f;
        }
    }
    
    private void RegistrarEnSistemaFisica()
    {
        if (SistemaFisica.instancia != null)
        {
            // Masa alta para que sean prácticamente inamovibles
            float masaEstatica = 1000f;
            SistemaFisica.instancia.RegistrarObjeto(gameObject, masaEstatica, radio, Vector2.zero);
            Debug.Log($"[Obstaculo] {gameObject.name} registrado en el sistema de física. Radio: {radio}");
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
            Debug.Log($"[Obstaculo] {gameObject.name} golpeado por bola");
            
            // Si es un súper rebote, aplicar fuerza extra a la bola
            if (tipo == TipoObstaculo.SuperRebote)
            {
                Vector2 direccionRebote = (otroObjeto.transform.position - transform.position).normalized;
                SistemaFisica.instancia.AplicarFuerza(otroObjeto, direccionRebote * SuperRebote);
            }
        }
    }
    
    void OnDestroy()
    {
        // Eliminar del sistema de física cuando se destruye
        if (SistemaFisica.instancia != null)
        {
            SistemaFisica.instancia.EliminarObjeto(gameObject);
        }
    }
}