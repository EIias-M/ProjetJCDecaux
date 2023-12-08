package com.soap.ws.client.exec;

import com.soap.ws.client.generated.IServerService;
import com.soap.ws.client.generated.ServerService;
import com.soap.ws.client.generated.ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM;

import javax.jms.*;
import javax.swing.*;
import java.util.List;
import java.util.Scanner;
import org.jxmapviewer.JXMapViewer;
import org.apache.activemq.ActiveMQConnection;
import org.apache.activemq.ActiveMQConnectionFactory;
import org.apache.activemq.command.ActiveMQQueue;

public class Main {

        public static void main(String[] args) {

            ServerService serverService = new ServerService();
            IServerService iServerService = serverService.getBasicHttpBindingIServerService();
            Scanner scanner = new Scanner(System.in);
            JFrame frame = new JFrame("Itineraire");
            JXMapViewer mapViewer = new JXMapViewer();

            while(true){
                System.out.println("Entrée l'adresse de départ : ");
                String depart = scanner.nextLine();
                System.out.println("Entrée l'adresse d'arrivée : ");
                String arrivee = scanner.nextLine();
                System.out.println("Calcul de l'itinéraire en cours ...\n");

                ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM result = iServerService.findWay(depart, arrivee);

                frame = Map.launchMap(result,frame,mapViewer);
                messageFromActiveMQ(result);
            }
        }

    private static void messageFromActiveMQ(ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM idQueue) {
        try {

            String brokerUrl = "tcp://localhost:61616";
            int ms = 20;
            if(idQueue.getItem2().getString().size()>=1000) ms = 5;

            // Configuration de la connexion ActiveMQ
            ConnectionFactory connectionFactory = new ActiveMQConnectionFactory(brokerUrl);
            Connection connection = connectionFactory.createConnection();
            connection.start();


            // Configuration de la session ActiveMQ
            Session session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
            Queue destination = session.createQueue(idQueue.getItem1());

            // Configuration du consommateur ActiveMQ
            MessageConsumer consumer = session.createConsumer(destination);



            boolean encore = true;
            //boucle tant que la queue n'est pas vide
            while(encore) {
                Thread.sleep(ms);
                Message message = consumer.receive(); // Cette méthode bloque jusqu'à ce qu'un message soit reçu
                //affichage du message
                if (message instanceof TextMessage) {
                    TextMessage textMessage = (TextMessage) message;
                    String receivedMessage = textMessage.getText();
                    //si la string contient fin du trajet alors c'est le dernier message.
                    if(receivedMessage.contains("Fin du trajet.")){
                        encore = false;
                    }else {
                        System.out.println(receivedMessage);
                    }
                }
            }
            //on ferme tout
            consumer.close();
            connection.close();
        } catch (Exception ex) {
            //affichage de l'itineraire si activeMq indisponible
            withoutActiveMQ(idQueue.getItem2().getString());
        }
        //on détruit la queue qui est maintenant vide
        ActiveMQConnection conn = null;
        try {
            conn = (ActiveMQConnection) new ActiveMQConnectionFactory("tcp://localhost:61616").createConnection();
            conn.destroyDestination(new ActiveMQQueue(idQueue.getItem1()));
            conn.close();
        } catch (JMSException e) {
            System.out.println(e.getMessage());
        }

    }

    private static void withoutActiveMQ(List<String> itineraire)  {
        int ms = 20;
        if(itineraire.size()>=1000) ms = 5;
        //affichage ligne par ligne de l'itinéraire
        try {
            for (String s : itineraire) {
                Thread.sleep(ms);
                System.out.println(s);
            }
        }
        catch (Exception ex){

            System.out.println(ex.getMessage());
        }
    }
}
