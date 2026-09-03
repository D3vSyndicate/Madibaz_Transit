
import { useState } from 'react';
import {
    ActivityIndicator,
    Alert,
    Pressable,
    StyleSheet,
    Text,
    TextInput,
    View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

export default function LoginScreen() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    const handleLogin = async () => {
        if (!email.trim() || !password) {
            Alert.alert(
                'Missing information',
                'Please enter your email and password.'
            );
            return;
        }

        setLoading(true);

        try {
            // API connection will be added after the screen is working.
            console.log('Login:', email);
        } catch {
            Alert.alert(
                'Login failed',
                'Something went wrong. Please try again.'
            );
        } finally {
            setLoading(false);
        }
    };

    return (
        <SafeAreaView style={styles.safeArea}>
            <View style={styles.container}>

                <View style={styles.header}>
                    <Text style={styles.title}>MADIBA TRANSIT</Text>
                    <Text style={styles.subtitle}>
                        Nelson Mandela University
                    </Text>
                </View>

                <View style={styles.form}>
                    <Text style={styles.heading}>Student Login</Text>

                    <Text style={styles.label}>University Email</Text>

                    <TextInput
                        style={styles.input}
                        placeholder="student@mandela.ac.za"
                        placeholderTextColor="#777777"
                        keyboardType="email-address"
                        autoCapitalize="none"
                        autoCorrect={false}
                        value={email}
                        onChangeText={setEmail}
                    />

                    <Text style={styles.label}>Password</Text>

                    <TextInput
                        style={styles.input}
                        placeholder="Enter your password"
                        placeholderTextColor="#777777"
                        secureTextEntry
                        value={password}
                        onChangeText={setPassword}
                    />

                    <Pressable
                        style={({ pressed }) => [
                            styles.loginButton,
                            pressed && styles.buttonPressed,
                        ]}
                        onPress={handleLogin}
                        disabled={loading}
                    >
                        {loading ? (
                            <ActivityIndicator color="#FFFFFF" />
                        ) : (
                            <Text style={styles.loginButtonText}>
                                LOGIN
                            </Text>
                        )}
                    </Pressable>
                </View>

                <Text style={styles.footer}>
                    Nelson Mandela University
                </Text>

            </View>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    safeArea: {
        flex: 1,
        backgroundColor: '#FFFFFF',
    },

    container: {
        flex: 1,
        paddingHorizontal: 28,
        paddingVertical: 40,
        justifyContent: 'space-between',
    },

    header: {
        alignItems: 'center',
        marginTop: 50,
    },

    title: {
        fontSize: 28,
        fontWeight: '700',
        color: '#002855',
        textAlign: 'center',
    },

    subtitle: {
        marginTop: 8,
        fontSize: 15,
        color: '#555555',
        textAlign: 'center',
    },

    form: {
        width: '100%',
    },

    heading: {
        fontSize: 24,
        fontWeight: '600',
        color: '#002855',
        marginBottom: 28,
    },

    label: {
        fontSize: 14,
        fontWeight: '600',
        color: '#333333',
        marginBottom: 8,
    },

    input: {
        height: 52,
        borderWidth: 1,
        borderColor: '#CCCCCC',
        borderRadius: 10,
        paddingHorizontal: 16,
        fontSize: 16,
        color: '#111111',
        backgroundColor: '#FFFFFF',
        marginBottom: 20,
    },

    loginButton: {
        height: 52,
        borderRadius: 10,
        backgroundColor: '#002855',
        alignItems: 'center',
        justifyContent: 'center',
        marginTop: 8,
        borderBottomWidth: 4,
        borderBottomColor: '#F2C500',
    },

    buttonPressed: {
        opacity: 0.8,
    },

    loginButtonText: {
        color: '#FFFFFF',
        fontSize: 16,
        fontWeight: '700',
    },

    footer: {
        textAlign: 'center',
        fontSize: 13,
        color: '#777777',
    },
});