# React Native Integration Guide

## 🚀 Kết nối React Native với HRM Cyberse Backend

### Backend Configuration (✅ Đã setup)

Backend đã được cấu hình với:
- ✅ CORS enabled cho React Native
- ✅ JWT authentication
- ✅ RESTful APIs
- ✅ Swagger documentation

---

## 📱 React Native Setup

### 1. Tạo React Native Project

```bash
npx react-native init HRMCyberseApp
cd HRMCyberseApp
```

### 2. Install Dependencies

```bash
npm install axios
npm install @react-native-async-storage/async-storage
npm install @react-navigation/native
npm install @react-navigation/stack
npm install react-native-gesture-handler
npm install react-native-reanimated
npm install react-native-screens
npm install react-native-safe-area-context
```

### 3. API Configuration

Tạo file `src/config/api.js`:

```javascript
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

// Backend URL
const API_BASE_URL = 'http://10.0.2.2:5267/api'; // Android Emulator
// const API_BASE_URL = 'http://localhost:5267/api'; // iOS Simulator
// const API_BASE_URL = 'http://YOUR_IP:5267/api'; // Physical Device

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add JWT token
api.interceptors.request.use(
  async (config) => {
    const token = await AsyncStorage.getItem('jwt_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token expired, logout user
      await AsyncStorage.removeItem('jwt_token');
      // Navigate to login screen
    }
    return Promise.reject(error);
  }
);

export default api;
```

---

## 🔐 Authentication Service

Tạo file `src/services/authService.js`:

```javascript
import api from '../config/api';
import AsyncStorage from '@react-native-async-storage/async-storage';

export const authService = {
  // Login
  login: async (username, password) => {
    try {
      const response = await api.post('/auth/login', {
        username,
        password,
      });
      
      if (response.data.success) {
        // Save token
        await AsyncStorage.setItem('jwt_token', response.data.token);
        await AsyncStorage.setItem('user', JSON.stringify(response.data.user));
        return response.data;
      }
      throw new Error(response.data.message);
    } catch (error) {
      throw error;
    }
  },

  // Register
  register: async (userData) => {
    try {
      const response = await api.post('/auth/register', userData);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  // Logout
  logout: async () => {
    await AsyncStorage.removeItem('jwt_token');
    await AsyncStorage.removeItem('user');
  },

  // Get current user
  getCurrentUser: async () => {
    const userJson = await AsyncStorage.getItem('user');
    return userJson ? JSON.parse(userJson) : null;
  },
};
```

---

## 📍 Attendance Service (Check-in/Check-out)

Tạo file `src/services/attendanceService.js`:

```javascript
import api from '../config/api';
import Geolocation from '@react-native-community/geolocation';

export const attendanceService = {
  // Check-in
  checkIn: async (userId, shiftId, imageUrl, notes) => {
    try {
      // Get GPS location
      const position = await getCurrentPosition();
      
      const response = await api.post('/attendance/check-in', {
        userId,
        shiftId,
        latitude: position.coords.latitude,
        longitude: position.coords.longitude,
        imageUrl,
        notes,
      });
      
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  // Check-out
  checkOut: async (attendanceId, imageUrl, notes) => {
    try {
      const position = await getCurrentPosition();
      
      const response = await api.post('/attendance/check-out', {
        attendanceId,
        latitude: position.coords.latitude,
        longitude: position.coords.longitude,
        imageUrl,
        notes,
      });
      
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  // Get today's attendance
  getTodayAttendance: async (userId) => {
    try {
      const response = await api.get(`/attendance/today/${userId}`);
      return response.data;
    } catch (error) {
      throw error;
    }
  },

  // Get attendance history
  getHistory: async (userId, startDate, endDate) => {
    try {
      const response = await api.get(`/attendance/history/${userId}`, {
        params: { startDate, endDate },
      });
      return response.data;
    } catch (error) {
      throw error;
    }
  },
};

// Helper function to get GPS location
const getCurrentPosition = () => {
  return new Promise((resolve, reject) => {
    Geolocation.getCurrentPosition(
      (position) => resolve(position),
      (error) => reject(error),
      { enableHighAccuracy: true, timeout: 20000, maximumAge: 1000 }
    );
  });
};
```

---

## 📝 Request Service (Leave, Shift, Late)

Tạo file `src/services/requestService.js`:

```javascript
import api from '../config/api';

export const requestService = {
  // Leave Requests
  createLeaveRequest: async (userId, startDate, endDate, reason) => {
    const response = await api.post('/requests/leave', {
      userId,
      startDate,
      endDate,
      reason,
    });
    return response.data;
  },

  getUserLeaveRequests: async (userId, status = null) => {
    const response = await api.get(`/requests/leave/user/${userId}`, {
      params: { status },
    });
    return response.data;
  },

  // Shift Requests
  createShiftRequest: async (userId, shiftId, shiftDate, reason) => {
    const response = await api.post('/shiftrequests', {
      userId,
      shiftId,
      shiftDate,
      reason,
    });
    return response.data;
  },

  // Late Requests
  createLateRequest: async (userId, shiftId, requestDate, expectedArrivalTime, reason) => {
    const response = await api.post('/laterequests', {
      userId,
      shiftId,
      requestDate,
      expectedArrivalTime,
      reason,
    });
    return response.data;
  },

  // Cancel request
  cancelRequest: async (requestId, type) => {
    const endpoint = type === 'leave' 
      ? `/requests/leave/${requestId}/cancel`
      : type === 'shift'
      ? `/shiftrequests/${requestId}/cancel`
      : `/laterequests/${requestId}/cancel`;
    
    const response = await api.post(endpoint);
    return response.data;
  },
};
```

---

## 💰 Payroll Service

Tạo file `src/services/payrollService.js`:

```javascript
import api from '../config/api';

export const payrollService = {
  // Get user payroll
  getUserPayroll: async (userId, month, year) => {
    const response = await api.get(`/payroll/user/${userId}`, {
      params: { month, year },
    });
    return response.data;
  },

  // Get payroll history
  getPayrollHistory: async (userId) => {
    const response = await api.get(`/payroll/user/${userId}/history`);
    return response.data;
  },

  // Get rewards/penalties
  getRewardsPenalties: async (userId, month, year) => {
    const response = await api.get(`/rewardpenalty/user/${userId}`, {
      params: { month, year },
    });
    return response.data;
  },
};
```

---

## 📸 Image Upload (Cloudinary)

Tạo file `src/services/imageService.js`:

```javascript
import { launchCamera, launchImageLibrary } from 'react-native-image-picker';

export const imageService = {
  // Take photo
  takePhoto: async () => {
    const result = await launchCamera({
      mediaType: 'photo',
      quality: 0.8,
      maxWidth: 1024,
      maxHeight: 1024,
    });

    if (result.assets && result.assets[0]) {
      return result.assets[0];
    }
    return null;
  },

  // Upload to Cloudinary
  uploadToCloudinary: async (imageUri) => {
    const formData = new FormData();
    formData.append('file', {
      uri: imageUri,
      type: 'image/jpeg',
      name: 'attendance.jpg',
    });
    formData.append('upload_preset', 'YOUR_UPLOAD_PRESET');

    try {
      const response = await fetch(
        'https://api.cloudinary.com/v1_1/YOUR_CLOUD_NAME/image/upload',
        {
          method: 'POST',
          body: formData,
        }
      );

      const data = await response.json();
      return data.secure_url;
    } catch (error) {
      throw error;
    }
  },
};
```

---

## 📱 Example Screens

### Login Screen

```javascript
import React, { useState } from 'react';
import { View, TextInput, Button, Alert } from 'react-native';
import { authService } from '../services/authService';

const LoginScreen = ({ navigation }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    try {
      setLoading(true);
      const result = await authService.login(username, password);
      
      if (result.success) {
        navigation.replace('Home');
      } else {
        Alert.alert('Error', result.message);
      }
    } catch (error) {
      Alert.alert('Error', 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={{ padding: 20 }}>
      <TextInput
        placeholder="Username"
        value={username}
        onChangeText={setUsername}
        style={{ borderWidth: 1, padding: 10, marginBottom: 10 }}
      />
      <TextInput
        placeholder="Password"
        value={password}
        onChangeText={setPassword}
        secureTextEntry
        style={{ borderWidth: 1, padding: 10, marginBottom: 10 }}
      />
      <Button title="Login" onPress={handleLogin} disabled={loading} />
    </View>
  );
};

export default LoginScreen;
```

### Check-in Screen

```javascript
import React, { useState, useEffect } from 'react';
import { View, Button, Text, Alert } from 'react-native';
import { attendanceService } from '../services/attendanceService';
import { imageService } from '../services/imageService';

const CheckInScreen = () => {
  const [todayAttendance, setTodayAttendance] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadTodayAttendance();
  }, []);

  const loadTodayAttendance = async () => {
    try {
      const userId = 1; // Get from AsyncStorage
      const data = await attendanceService.getTodayAttendance(userId);
      setTodayAttendance(data);
    } catch (error) {
      console.log('No attendance today');
    }
  };

  const handleCheckIn = async () => {
    try {
      setLoading(true);
      
      // Take photo
      const photo = await imageService.takePhoto();
      if (!photo) return;

      // Upload to Cloudinary
      const imageUrl = await imageService.uploadToCloudinary(photo.uri);

      // Check-in
      const result = await attendanceService.checkIn(
        1, // userId
        1, // shiftId
        imageUrl,
        'Checked in from mobile'
      );

      Alert.alert('Success', 'Checked in successfully!');
      loadTodayAttendance();
    } catch (error) {
      Alert.alert('Error', 'Check-in failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCheckOut = async () => {
    try {
      setLoading(true);
      
      const photo = await imageService.takePhoto();
      if (!photo) return;

      const imageUrl = await imageService.uploadToCloudinary(photo.uri);

      const result = await attendanceService.checkOut(
        todayAttendance.id,
        imageUrl,
        'Checked out from mobile'
      );

      Alert.alert('Success', 'Checked out successfully!');
      loadTodayAttendance();
    } catch (error) {
      Alert.alert('Error', 'Check-out failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={{ padding: 20 }}>
      {todayAttendance ? (
        <View>
          <Text>Check-in: {todayAttendance.checkInTime}</Text>
          <Text>Status: {todayAttendance.status}</Text>
          {!todayAttendance.checkOutTime && (
            <Button title="Check Out" onPress={handleCheckOut} disabled={loading} />
          )}
        </View>
      ) : (
        <Button title="Check In" onPress={handleCheckIn} disabled={loading} />
      )}
    </View>
  );
};

export default CheckInScreen;
```

---

## 🔧 Testing Connection

### 1. Start Backend
```bash
cd HRMCyberse
dotnet run
```

### 2. Test API từ React Native

```javascript
// Test connection
import api from './config/api';

const testConnection = async () => {
  try {
    const response = await api.get('/shifts');
    console.log('Connection successful!', response.data);
  } catch (error) {
    console.error('Connection failed:', error);
  }
};
```

---

## 📝 Important Notes

### Android Emulator
- Use `http://10.0.2.2:5267/api` để connect tới localhost

### iOS Simulator
- Use `http://localhost:5267/api`

### Physical Device
- Backend phải chạy trên cùng WiFi
- Use `http://YOUR_COMPUTER_IP:5267/api`
- Ví dụ: `http://192.168.1.100:5267/api`

### HTTPS
- Development: Có thể dùng HTTP
- Production: BẮT BUỘC dùng HTTPS

---

## 🚀 Next Steps

1. **Setup React Native project**
2. **Install dependencies**
3. **Copy service files**
4. **Configure API base URL**
5. **Test connection**
6. **Build UI screens**
7. **Test all features**

---

## 📚 Additional Resources

- React Native Docs: https://reactnative.dev/
- Axios Docs: https://axios-http.com/
- React Navigation: https://reactnavigation.org/
- Cloudinary Upload: https://cloudinary.com/documentation

---

**Ready to build your React Native app!** 🎉
