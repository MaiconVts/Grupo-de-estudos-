import React, { useState, useEffect } from 'react';
import { StyleSheet, Text, View, Image } from 'react-native';
import { useRoute } from '@react-navigation/native';
import Logo from '../../../../assets/images/Logo.png';
import { getReport } from "../../services/apiService";

export default function RelatorioAluno() {
    const route = useRoute();
    const nomeAluno = route.params?.nomeAluno;
    const nomeTurma = route.params?.nomeTurma;
    const [relatorioData, setRelatorioData] = useState([]);

    useEffect(() => {
        getRelatorios();
    }, []);

    const getRelatorios = async () => {
        try {
            console.log("Passei por aqui")
            const response = await getReport(nomeAluno, nomeTurma);
            setRelatorioData(response.report);
        } catch (error) {
            console.log("Erro ao recuperar os relatórios", error);
        }
    };

    return (
        <View style={styles.container}>
            <Image style={styles.logo} source={Logo} />
            <Text style={styles.titleTela}>Relatório</Text>
            <View style={styles.containerTurma}>
                <View style={styles.containerAluno}>
                    <Text style={styles.titleAlunos}>{nomeAluno}</Text>
                </View>
                <View style={styles.containerRelatorio}>
                    {relatorioData.length > 0 ? (
                        <>
                            <Text style={styles.faltas}>
                                Total de Faltas:{" "}
                                {(
                                    relatorioData.filter((item) => item.attendance === 0).length *
                                    2.5
                                ).toFixed(2)}%
                            </Text>

                            {relatorioData.map((item, index) => (
                                <View key={index} style={styles.relatorioItem}>
                                    <Text style={styles.materia}>{item.subject}</Text>
                                    <Text>Notas: {item.grades}</Text>
                                </View>
                            ))}
                        </>
                    ) : (
                        <Text>Aguardando dados...</Text>
                    )}
                </View>

            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        margin: 20,
        top: 10,
    },
    containerTurma: {
        backgroundColor: '#C0C2C4',
        margin: 10,
        borderRadius: 15,
        height: 600,
    },
    containerAluno: {
        justifyContent: 'center',
        alignItems: 'center',
        marginTop: 20,
    },
    titleAlunos: {
        fontSize: 30,
    },
    titleTela: {
        fontSize: 50,
        paddingLeft: 85,
        marginBottom: 20,
    },
    logo: {
        marginLeft: 310,
        height: 50,
        width: 60,
    },
    containerRelatorio: {},
    relatorioItem: {
        marginBottom: 20,
        marginLeft: 40
    },
    Button: {
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#96CA5E',
        height: 40,
        width: 110,
        borderRadius: 12,
        marginLeft: 220,
        bottom: 30
    },
    materia: {
        justifyContent: 'center',
        alignItems: 'center',
        fontSize: 17
    },
    faltas: {
        marginLeft: 200,
        top: 495
    }
});
