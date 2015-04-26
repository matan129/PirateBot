package bots;


import java.util.*;

import pirates.game.*;
public class testing {
	
	public static void main(String[] args) {
		/*
		Location loc=new Location(58, 15);
		Island isle=new Island(0, loc, 0, 0, 0, 0, 0);
		Target tr=new Target(isle);
		System.out.println(tr.getType());
		System.out.println(tr.getType().toString().contains("Island"));
		System.out.println(tr.getLocation());
		System.out.print(tr.getPriority()+" : ");
		System.out.println(tr.getPriority().getValue());
		tr.changePriority(-1);
		System.out.print(tr.getPriority()+" : ");
		System.out.println(tr.getPriority().getValue());
		Navy myNavy=new Navy(0);
		Target myNavyTarget=new Target(myNavy);
		System.out.println(myNavyTarget);


		HashSet<Integer> myTable=new HashSet<Integer>();
		myTable.add(5);
		myTable.add(80);
		myTable.add(-100);
		myTable.add(5);
		System.out.println(myTable.toString());
		System.out.println(Collections.max(myTable));
		System.out.println(Collections.min(myTable));
		
		List<Integer> distanceList=new ArrayList<Integer>();
		distanceList.add(50);
		distanceList.add(786);
		distanceList.add(10);
		distanceList.add(-20);
		distanceList.add(2034);
		distanceList.sort((distance1, distance2) -> distance2.compareTo(distance1));
		Collections.sort(distanceList);
		System.out.println(distanceList.toString());
		System.out.println("index 0 : " + distanceList.get(0)); //max
		System.out.println("last index : " + distanceList.get(distanceList.size()-1)); //min
		System.out.println(MyBot.game.getAttackRadius());
		
		
		Navy newNavy=new Navy(5);
		Target newTarget=new Target(newNavy);
		System.out.println(newTarget.getType());
		*/
		Hashtable<String, Integer> hello=new Hashtable<String, Integer>();
		hello.put("Matan", 16);
		hello.put("Uri", 17);
		Target myTarget=new Target(hello);
		System.out.println(myTarget.getObject().toString());
	}
}
