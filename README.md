# Sword Fu

Github: https://github.com/Xavierlm11/Sword-Fu
(Remake Branch)

-21/11/2023:
[Novedades]
En esta versión hemos prescindido del modo de conexión TCP para centrarnos solamente en UDP.
Hemos reestructrado todo el código para que ambos clientes sean capaces tanto de recibir información como de enviarla.
Además, también hemos rehecho el Lobby y conectado con una base de Gameplay.

[Funcionamiento]
Inicia dos instancias de la aplicación. Una será el host y otra un cliente, uniéndose en ese orden. El host le dará al
botón de Create Game y podrá ponerse un nickname. También puede configurar el puerto que usará (Server Port) o el que deben
usar los clientes que se conecten a él (Client Port). Ambos campos tienen un botón de Default para establecerlos en 
5000 y 6000 respectivamente, lo cual viene por defecto y en la mayoría de ocasiones no es necesario cambiar. El botón de 
"Get Auto" obtendrá un puerto libre automáticamente, aunque ese puerto podría ocuparse al poco rato por algún proceso externo,
 por lo que habría que generar uno nuevo. Cuando estés listo, el botón "Create Room" te llevará a la sala de espera.
Por el lado del cliente, los campos de Nickname y puertos son iguales, pero hay un campo IP en el que hay que introducir la 
IP del servidor, y al finalizar hay que pulsar el botón "Join", el cual te llevará a la sala de espera con el servidor.

[Estructura]
El código se basa principalmente en varios Singleton. El primero (que también es un ScriptableObject) se llama NetworkManager
y contiene datos de la configuración de la aplicación y clases básicas como la de cliente y otras usadas para el traspaso
de información entre clientes, además de datos sobre el cliente local. El LobbyManager maneja los menús y lo que ocurre cuando
 el usuario interactúa con botones o InputFields. El ConnectionManager gestiona los sockets que usan los clientes y las 
conexiones entre estos. El PartyManager gestion la lista de jugadores que hay en la sala y el PlayerManager controla la generación
de jugadores y la actualización de sus posiciones.


[Problemas]
Teníamos un error que no supimos exactamente como solventar, ya que este se producía al llamar a la función Socket.Bind()
 por segunda vez con la misma dirección IP y puerto, cosa que hacíamos a la hora de crear varias instancias de la aplicación
en un mismo ordenador e intentar conectar los clientes, los cuales tenían la misma IP y puerto. Un Fix provisional ha sido
hacer que la aplicación use dos puertos. El host usará un puerto y el cliente otro, los cuales pueden ser modificados en la
aplicación. Aún así, la aplicación solo ha podido ser testeada con dos únicas instancias y en un mismo ordenador, y es 
importante que el Host se conecte primero y el otro cliente segundo.


[Main Tasks]
-Xavi C: Reestructuración de todo el código y las conexiones entre clientes, reformas en los menús.
-Xavi L: Reestructuración de los menús y conexiones entre estos y el Gameplay.
-Albert M: Gameplay

[Used Packages]

-Newtonsoft Json: Hemos usado este package para poder serializar objectos dentro de otros objetos y ponerlos en un Json.
