package org.example;

import com.example.generated.InterfaceSoap;
import com.example.generated.SoapService;
import org.apache.activemq.ActiveMQConnectionFactory;

import javax.jms.*;
import java.util.Scanner;

// Press Shift twice to open the Search Everywhere dialog and type `show whitespaces`,
// then press Enter. You can now see whitespace characters in your code.
public class Main {
    private static final String activemq_URL = "tcp://localhost:61616";
    private static final String queue_NAME = "InstructionQueue";
    public static void main(String[] args) {
        SoapService proxy = new SoapService();
        InterfaceSoap port = proxy.getBasicHttpBindingInterfaceSoap();

        try {
            //l'exemple de requête à lancer
            String originLat = "45.7507059";
            String originLon = "4.8278612";
            String destinationLat = "45.7671646";
            String destinationLon = "4.8334725";
            String modeBike = "bike";
            String itinerary = port.getGeneratedItinerary(originLat, originLon, destinationLat, destinationLon, modeBike);
            System.out.println("SOAP Itinerary: " + itinerary);
        } catch (Exception e) {
            e.printStackTrace();
        }

        // Connexion ActiveMQ
        ActiveMQConnectionFactory connectionFactory = new ActiveMQConnectionFactory(activemq_URL);
        Connection connection = null;
        try {
            connection = connectionFactory.createConnection();
            connection.start();

            // Créer une session et destination
            Session session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
            Destination destination = session.createQueue(queue_NAME);

            // Créer le consommateur
            MessageConsumer consumer = session.createConsumer(destination);

            System.out.println("Connexion à ActiveMQ réussie.");
            System.out.println("====================================");
            System.out.println("   EN ATTENTE DE MESSAGES           ");
            System.out.println("====================================");

            Scanner scanner = new Scanner(System.in);
            boolean keepRunning = true;

            while (keepRunning) {
                System.out.println("1. Récupérer une nouvelle instruction");
                System.out.println("2. Quitter");
                System.out.print("Choisissez une option : ");

                String choice = scanner.nextLine();
                switch (choice) {
                    case "1":
                        // Récupérer un message
                        TextMessage message = (TextMessage) consumer.receive(5000); // Timeout après 5 secondes
                        if (message != null) {
                            System.out.println("---> Instruction Reçue: " + message.getText());
                        } else {
                            System.out.println("Pas de nouveaux messages. Veuillez réessayer.");
                        }
                        break;

                    case "2":
                        System.out.println("Fermeture de l'application...");
                        keepRunning = false;
                        break;

                    default:
                        System.out.println("Option invalide. Veuillez réessayer.");
                }
            }
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