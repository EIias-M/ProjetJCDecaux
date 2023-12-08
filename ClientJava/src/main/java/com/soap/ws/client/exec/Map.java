package com.soap.ws.client.exec;
import java.awt.*;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import javax.swing.*;
import javax.swing.event.MouseInputListener;

import com.soap.ws.client.generated.*;
import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.input.CenterMapListener;
import org.jxmapviewer.input.PanKeyListener;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.input.ZoomMouseWheelListenerCursor;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.DefaultWaypoint;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;
import org.jxmapviewer.viewer.Waypoint;
import org.jxmapviewer.viewer.WaypointPainter;

/**
 * A simple sample application that shows
 * a OSM map of Europe containing a route with waypoints
 * @author Martin Steiger
 */
public class Map implements Runnable
{
    ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM points;
    JFrame frame = null;
    JXMapViewer mapViewer = new JXMapViewer();
    public Map(ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM points,JFrame frame,JXMapViewer mapViewer){
        this.points = points;
        this.frame = frame ;
        this.mapViewer = mapViewer;
    }
    public static JFrame launchMap(ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM points,JFrame frame,JXMapViewer mapViewer){
        if(points.getItem3() !=null) {

            Map map = new Map(points,frame,mapViewer);

            Thread thread = new Thread(map);
            thread.start();
        }
        return frame;
    }

    @Override
    public void run() {


        // Display the viewer in a JFrame
        frame.getContentPane().removeAll();

        frame.setSize(1600, 900);
        frame.getContentPane().add(mapViewer);
        JList<String> messageIti = new JList<>(points.getItem2().getString().toArray(new String[0]));
        JScrollPane scrollPane = new JScrollPane(messageIti);
        JSplitPane splitPane = new JSplitPane(JSplitPane.HORIZONTAL_SPLIT, scrollPane,mapViewer );
        splitPane.setResizeWeight(0.1);
        frame.getContentPane().add(splitPane);

        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setVisible(true);

        MouseInputListener mia = new PanMouseInputListener(mapViewer);
        mapViewer.addMouseListener(mia);
        mapViewer.addMouseMotionListener(mia);

        mapViewer.addMouseListener(new CenterMapListener(mapViewer));

        mapViewer.addMouseWheelListener(new ZoomMouseWheelListenerCursor(mapViewer));

        mapViewer.addKeyListener(new PanKeyListener(mapViewer));
        // Create a TileFactoryInfo for OpenStreetMap
        TileFactoryInfo info = new OSMTileFactoryInfo();
        DefaultTileFactory tileFactory = new DefaultTileFactory(info);
        mapViewer.setTileFactory(tileFactory);
        List<GeoPosition> track = new ArrayList<>();
        Set<Waypoint> waypoints = new HashSet<>();

        int i = 0;
        DefaultWaypoint suppr = null;
        for(ArrayOfArrayOfdouble gs : points.getItem3().getArrayOfArrayOfdouble()) {
            for (ArrayOfdouble g : gs.getArrayOfdouble()) {
                GeoPosition p = new GeoPosition(g.getDouble().get(1), g.getDouble().get(0));
                track.add(p);

                if (g == gs.getArrayOfdouble().get(0)){
                    i++;
                    DefaultWaypoint wp = new DefaultWaypoint(p);
                    waypoints.add(wp);
                    if (i == 2) {
                        suppr = wp;
                    }
                }

            }
        }
        waypoints.remove(suppr);
        RoutePainter routePainter = new RoutePainter(track);

        // Set the focus
        mapViewer.zoomToBestFit(new HashSet<GeoPosition>(track), 0.7);


        // Create a waypoint painter that takes all the waypoints
        WaypointPainter<Waypoint> waypointPainter = new WaypointPainter<Waypoint>();
        waypointPainter.setWaypoints(waypoints);

        // Create a compound painter that uses both the route-painter and the waypoint-painter
        List<Painter<JXMapViewer>> painters = new ArrayList<Painter<JXMapViewer>>();
        painters.add(routePainter);
        painters.add(waypointPainter);

        CompoundPainter<JXMapViewer> painter = new CompoundPainter<JXMapViewer>(painters);
        mapViewer.setOverlayPainter(painter);
    }
}