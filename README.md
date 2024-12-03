# LetsGoBiking

## Description
This project was developed as part of the Middleware and Service-Oriented Computing course and focuses on building a self-hosted SOAP server in C# with advanced features such as caching, message queuing, and proxy handling. The server is complemented by a web client designed using HTML, CSS, and JavaScript, providing an interactive interface for itinerary planning.

The SOAP server integrates data from the JC Decaux API and an external REST API to process and deliver routing services. The web client enables users to submit requests and visualize optimized itineraries through an intuitive and responsive design.

The project illustrates the practical application of middleware concepts, showcasing the interoperability of SOAP and RESTful services. It highlights the importance of service-oriented architecture (SOA) in integrating diverse technologies into a cohesive system, focusing on real-time data processing, effective client-server interaction, and seamless API integration.

## 🌟 Key Features
- **Plan your journey:** Input any origin and destination for route calculation 🗺️
- **Interactive map visualization:** Display routes dynamically on an intuitive web interface 🌐
- **Seamless web experience:** User-friendly design built with HTML, CSS, and JavaScript 💻
- **Optimized travel planning:** Routes include navigation from your current location to the nearest station, and from the closest station to your final destination 🚉➡️📍
- **Self-hosted SOAP server:** Developed in C# to handle itinerary computation and data processing 🛠️
- **Integration with APIs:** Connects with the JC Decaux API for bike-sharing and external REST APIs for enhanced functionality 🚲🔗
- **Robust architecture:** Features advanced middleware capabilities, including caching and message queuing via ActiveMQ 📥
- **Comprehensive documentation:** Clear instructions for setup, usage, and troubleshooting 📖

## 🔑 Key Features of the System:
- 🔧 **RoutingServer:**
  - SOAP-based service for itinerary planning.
  - Integrated caching for optimized performance.
  - Proxy support for routing API requests.
- 🌟 **Web Client:**
  - Built with HTML, CSS, and JavaScript.
  - Provides a user-friendly interface for itinerary visualization.
- 🌟 **Heavy JAVA Client:**
  - Built with JAVA (maven project).
  - Provides a console-based display of itinerary instructions.

### Requirements

- .NET Framework 4.8
- Java version 21.0.3
- ActiveMQ
- Maven

### 🚀 Installation and step-by-step setup
>⚠️[WARNING]  
> Due to Windows 10/11 port access policies and protected system resources, you must run the servers as an administrator to allow them to host services on localhost. If you are running the servers from an IDE, ensure that you launch your IDE as an administrator as well.
  
> 💡 [NOTE]  
> Ensure that your environment variables are properly configured for tools like [msbuild](https://visualstudio.microsoft.com/downloads/?cid=learn-onpage-download-cta#build-tools-for-visual-studio-2022), [nuget](https://www.nuget.org/downloads), [activemq](https://activemq.apache.org/components/classic/download/) and [mvn](https://maven.apache.org/download.cgi)), If everything is set up, you can run the process automatically with `launch_project.bat`.

>⚠️[WARNING]    
> Regarding the execution of the Heavy Client in JAVA, it is done manually (as explained [further below](#steps-for-manual-execution-of-the-clientJAVA-project)).


### In case you don't want to run the 'launch_project.bat' file follow these steps below :

1. 🛠️ **Clone** the repository to your local machine.
```bash
git clone https://github.com/WalidELAJI/Web-Front-Development/
```

2. 🌐 **Start ActiveMQ:**
**Open** an ActiveMQ instance in a terminal. Ensure it is [you have it installed](https://activemq.apache.org/components/classic/download/) and added to your system's environment variables.
```bash
activemq start
```

3. ⚙️  **Set Up the RoutingServer:**
**Navigate** to the `RoutingServer` directory and **build** the solution. The RoutingServer integrates key components:
  - **SOAP server** for handling requests.
  - **Caching mechanism** to improve performance.
  - **Proxy functionality** for routing services.
```bash
cd RoutingServer
nuget restore RoutingServer.sln
msbuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.8
start "Routing Server" .RoutingServer\RoutingServer\bin\Debug\RoutingServer.exe
```

4. 💻 **Launch the Web Client:**
**Navigate** to the `src` directory and open the `index.html` file in your default web browser. This file serves as the front-end for displaying the routing services.
```bash
cd ../src
start index.html
```



## 💡 How to use

Once you did all the steps above, you'll be prompted to choose a starting place and a destination.

### Part 1 : Examples of the web front Interface

🔵 = JCDecaux bike itinerary
🔴 = Walking itinerary

#### Some map showcases and search addresses (note that you can type whatever you want)

```
Départ :
Place Carnot 69002 Lyon

Arrivée :
Place Bellecour 69002 Lyon

Mode :
BIKING
```

![BIKING](https://github.com/user-attachments/assets/644f9360-fb27-499d-816c-efbe985c6c0f)

---

```
Départ :
Place Carnot 69002 Lyon

Arrivée :
Place Bellecour 69002 Lyon

Mode :
WALKING
```

![WALKING](https://github.com/user-attachments/assets/2c4fbd1c-bb0a-4a0f-bae2-5900113d3d38)


### Part 2 : Examples of the Client Console execution

### Steps for manual execution of the ClientJAVA project

1. Open the `ClientJAVA` project in your IDE or terminal.
2. Navigate to the `Main.java` file within your project directory.
3. Execute the `Main.java` file manually
>⚠️[WARNING]    
> Do not attempt to execute the Main file using Maven, as some necessary dependencies may block the process.
4. Once the program starts, you will see the following messages in the console.

```
Connexion à ActiveMQ réussie.
====================================
   EN ATTENTE DE MESSAGES           
====================================
1. Récupérer une nouvelle instruction
2. Quitter
Choisissez une option : 

```
Once the user input 1 to ask for instructions for his itinerary :  
```
Choisissez une option : 1
---> Instruction Reçue: Head southeast
1. Récupérer une nouvelle instruction
2. Quitter
Choisissez une option :

```
The user can keep on asking for the next instructions until he gets all the instructions from the queue. 


## 🐛 Known issues

- **Heavy Client:** Currently, there is no visual interface with a map and route plotting, only the console is used to interact with the user. 
- **Routing Display:** The feature to display the best itinerary between biking and walking has not been implemented yet. However, the front-end does show the duration for each selected itinerary.


## ✍️ Authors

- **Walid El Aji** 
- **Yasmine Badia** 



