import { db } from './firebase';
import { doc, getDoc, setDoc, collection, addDoc, serverTimestamp } from 'firebase/firestore';

// Check if username exists and password matches — returns both apiKeys on success
export async function loginUser(username: string, password: string): Promise<{ success: boolean; message: string; geminiApiKey?: string; openRouterApiKey?: string }> {
  try {
    const userDoc = await getDoc(doc(db, 'users', username));
    
    if (!userDoc.exists()) {
      return { success: false, message: 'Username not found' };
    }
    
    const userData = userDoc.data();
    if (userData.password === password) {
      return { 
        success: true, 
        message: 'Login successful', 
        geminiApiKey: userData.geminiApiKey || userData.apiKey, // Backward compatibility
        openRouterApiKey: userData.openRouterApiKey || ''
      };
    } else {
      return { success: false, message: 'Incorrect password' };
    }
  } catch (error) {
    console.error('Login error:', error);
    return { success: false, message: 'Login failed. Please try again.' };
  }
}

// Create new user account with both API keys
export async function signupUser(username: string, password: string, geminiApiKey: string, openRouterApiKey: string): Promise<{ success: boolean; message: string }> {
  try {
    // Check if username already exists
    const userDoc = await getDoc(doc(db, 'users', username));
    
    if (userDoc.exists()) {
      return { success: false, message: 'Username already exists' };
    }
    
    // Create new user document with both API keys
    await setDoc(doc(db, 'users', username), {
      password: password,
      geminiApiKey: geminiApiKey,
      openRouterApiKey: openRouterApiKey,
      createdAt: serverTimestamp()
    });
    
    return { success: true, message: 'Account created successfully' };
  } catch (error) {
    console.error('Signup error:', error);
    return { success: false, message: 'Signup failed. Please try again.' };
  }
}

// Save name suggestion
export async function saveNameSuggestion(username: string, description: string, suggestedNames: string[]): Promise<{ success: boolean; message: string }> {
  try {
    await addDoc(collection(db, 'nameSuggestions'), {
      username: username,
      description: description,
      suggestedNames: suggestedNames,
      createdAt: serverTimestamp()
    });
    
    return { success: true, message: 'Name suggestion saved' };
  } catch (error) {
    console.error('Save suggestion error:', error);
    return { success: false, message: 'Failed to save suggestion' };
  }
}
