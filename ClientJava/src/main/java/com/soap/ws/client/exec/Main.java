package com.soap.ws.client.exec;

import com.soap.ws.client.generated.IServerService;
import com.soap.ws.client.generated.ServerService;

import java.util.Scanner;

public class Main {

        public static void main(String[] args) {

            ServerService serverService = new ServerService();
            IServerService iServerService = serverService.getBasicHttpBindingIServerService();
            Scanner scanner = new Scanner(System.in);

            System.out.println("Entrée l'adresse de départ : ");
            String depart = scanner.nextLine();
            System.out.println("Entrée l'adresse d'arrivée : ");
            String arrivee = scanner.nextLine();

            String result = iServerService.findWay(depart, arrivee);

            System.out.println(result);
        }
}
