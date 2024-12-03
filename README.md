# LetsGoBiking

## Description
This project was developed as part of the Middleware and Service-Oriented Computing course and focuses on building a self-hosted SOAP server in C# with advanced features such as caching, message queuing, and proxy handling. The server is complemented by a web client designed using HTML, CSS, and JavaScript, providing an interactive interface for itinerary planning.

The SOAP server integrates data from the JC Decaux API and an external REST API to process and deliver routing services. The web client enables users to submit requests and visualize optimized itineraries through an intuitive and responsive design.

The project illustrates the practical application of middleware concepts, showcasing the interoperability of SOAP and RESTful services. It highlights the importance of service-oriented architecture (SOA) in integrating diverse technologies into a cohesive system, focusing on real-time data processing, effective client-server interaction, and seamless API integration.

## 🌟 Key Features
- **Plan your journey:** Input any origin and destination for route calculation 🗺️
- **Interactive map visualization:** Display routes dynamically on an intuitive web interface 🌐
- **Live updates:** Real-time data refreshes with an automatically updating map 🔄
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

### Requirements

- .NET Framework 4.8
- Java 11+
- ActiveMQ
- Maven

### 🚀 Installation and step-by-step setup
>⚠️[WARNING]  
> Due to Windows 10/11 port access policies and protected system resources, you must run the servers as an administrator to allow them to host services on localhost. If you are running the servers from an IDE, ensure that you launch your IDE as an administrator as well.
  
> 💡 [NOTE]  
> Ensure that your environment variables are properly configured for tools like ([msbuild (https://visualstudio.microsoft.com/downloads/?cid=learn-onpage-download-cta#build-tools-for-visual-studio-2022), [nuget](https://www.nuget.org/downloads), [activemq](https://activemq.apache.org/components/classic/download/) and [mvn](https://maven.apache.org/download.cgi)), If everything is set up, you can run the process automatically with `launch_project.bat`.

1. 🛠️ **Clone** the repository to your local machine.

```bash
git clone https://github.com/WalidELAJI/Web-Front-Development/
```

2. 🌐 **Start ActiveMQ:**
**Open** an ActiveMQ instance in a terminal. Ensure it is [you have it installed](https://activemq.apache.org/components/classic/download/)and added to your system's environment variables.

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
  

