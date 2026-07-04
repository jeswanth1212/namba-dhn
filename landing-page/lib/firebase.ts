import { initializeApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";
import { getStorage } from "firebase/storage";

const firebaseConfig = {
  apiKey: "AIzaSyAERdi0nA3KIjqa96QyRhMGJgnZ8hPnbfU",
  authDomain: "namba-dhn.firebaseapp.com",
  projectId: "namba-dhn",
  storageBucket: "namba-dhn.firebasestorage.app",
  messagingSenderId: "589713391708",
  appId: "1:589713391708:web:435b5657d63905c4ee2699",
  measurementId: "G-24SYM0QSCM"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Firestore
export const db = getFirestore(app);

// Initialize Storage
export const storage = getStorage(app);
