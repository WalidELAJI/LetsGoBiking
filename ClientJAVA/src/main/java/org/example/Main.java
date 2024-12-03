package org.example;

import com.example.generated.InterfaceSoap;
import com.example.generated.SoapService;
import org.apache.activemq.ActiveMQConnectionFactory;

import javax.jms.*;
import java.util.Scanner;

// Press Shift twice to open the Search Everywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {
    private static final String BROKER_URL = "tcp://localhost:61616";
    private static final String QUEUE_NAME = "InstructionQueue";
    public static void main(String[] args) {
        SoapService proxy = new SoapService();
        InterfaceSoap port = proxy.getBasicHttpBindingInterfaceSoap();

        try {
            //l'exemple de requête à lancer
            String originLat = "48.8566";
            String originLon = "2.3522";
            String destinationLat = "51.5074";
            String destinationLon = "-0.1278";
            String modeBike = "bike";
            String itinerary = port.getGeneratedItinerary(originLat, originLon, destinationLat, destinationLon, modeBike);
            System.out.println("SOAP Itinerary: " + itinerary);
        } catch (Exception e) {
            e.printStackTrace();
        }
        // ActiveMQ Connection
        ActiveMQConnectionFactory connectionFactory = new ActiveMQConnectionFactory(BROKER_URL);
        Connection connection = null;
        try {
            // Create connection and start it
            connection = connectionFactory.createConnection();
            connection.start();
            // Create session and destination

            Session session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
            Destination destination = session.createQueue(QUEUE_NAME);
            // Create consumer
            MessageConsumer consumer = session.createConsumer(destination);
            System.out.println("En attente de messages de ActiveMQ...");
            Scanner scanner = new Scanner(System.in);
            boolean keepRunning = true;
            while (keepRunning) {
                // Receive message
                TextMessage message = (TextMessage) consumer.receive(5000); // Timeout after 5 seconds
                if (message != null) {
                    System.out.println("Instructions Reçue: " + message.getText());
                    System.out.println("Cliquer sur Entrer pour passer à l'instruction suivante OU entrer 'Q' pour quitter.");
                    String input = scanner.nextLine();
                    if ("Q".equalsIgnoreCase(input)) {
                        keepRunning = false;
                    }
                } else {
                    System.out.println("Pas de nouveaux messages. Attente...");
                }
            }
            System.out.println("Exiting message.");
        } catch (JMSException e) {
            e.printStackTrace();
        } finally {
            // Explicitly close the connection
            if (connection != null) {
                try {
                    connection.close();
                } catch (JMSException e) {
                    e.printStackTrace();
                }
            }
        }
    }
}