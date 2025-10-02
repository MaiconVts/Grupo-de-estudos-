import React, { useEffect,useState } from "react";
import { FlatList, StyleSheet, Text, View, Image, TouchableOpacity, Alert, Button, Modal, TextInput } from "react-native";
import { Button as ButtonPers } from "../../components/ButtonCadastro";
import { logout,getClasses, createClass, deleteClass, insertStudent } from "../../services/apiService";
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useNavigation } from "@react-navigation/native";
import { Ionicons } from '@expo/vector-icons';



export default function Turma(){
 
  const [turmasData, setTurmasData] = useState([]);
  const [newTurmaName, setNewTurmaName] = useState(""); 
  const [modalVisible, setModalVisible] = useState(false); 
  const [modalAlunoVisible, setModalAlunoVisible] = useState(false);
  const [selectedClassName, setSelectedClassName] = useState("");
  const [newStudentName, setNewStudentName] = useState("");
  const navigation = useNavigation();

   
  useEffect(()=>{
    checkSession();
    getTurmas();
  },[]);

  const handleAddTurma = async () => {
    if (newTurmaName.trim() === "") {
      Alert.alert("Erro", "O nome da turma não pode estar vazio.");
      return;
    }
    try{
      const data = {
        name: newTurmaName
      }
      const response = await createClass(data);
      setTurmasData([...turmasData, response]);
      setNewTurmaName("")
      setModalVisible(false); 
    }catch(error){
      console.log("Erro ao criar turma",error);
    }
  };

  const handleInsertStudent = async () => {
    if (newStudentName.trim() === "") {
      Alert.alert("Erro", "O nome do aluno não pode estar vazio.");
      return;
    }
    try {
      await insertStudent(selectedClassName, newStudentName);
      Alert.alert("Sucesso", "Aluno adicionado com sucesso.");
      setModalAlunoVisible(false);
      setNewStudentName("");
    } catch (error) {
      console.error("Erro ao inserir aluno", error);
      Alert.alert("Erro", "Não foi possível adicionar o aluno.");
    }
  };


  const handleDeleteTurma = async(id) => {
    Alert.alert(
      "Confirmação",
      "Tem certeza que deseja excluir esta turma?",
    
    [
      {
        text: "Cancelar",
        style: "cancel",
      },
      {
        text: "Excluir",
        onPress: async () => {
          try {
            await deleteClass(id); 
            setTurmasData((prevData) => prevData.filter((turma) => turma.id !== id)); 
            Alert.alert("Sucesso", "Turma excluída com sucesso.");
          } catch (error) {
            console.error("Erro ao excluir turma", error);
            Alert.alert("Erro", "Não foi possível excluir a turma.");
          }
        },
      },
    ]
  );
  }
  
  
  const getTurmas = async()=>{
    try{
      const response = await getClasses();
      setTurmasData(response);
    }catch(error){
      console.log("Erro ao recuperar as turmas",error);
    }
  };

  checkSession = async () => {
    try {
      const sessionData = await AsyncStorage.getItem("userSession");
      if (!sessionData) {
        navigation.replace("LoginScreen");
      }
    } catch (error) {
      console.error("Erro ao verificar sessão:", error);
      navigation.replace("LoginScreen");
    }
  };

  handleLogout = async () => {
    try {
      await logout();
      await AsyncStorage.removeItem("userSession");
      Alert.alert("Sucesso", "Você foi deslogado com sucesso.");
      navigation.replace("LoginScreen");
    } catch (error) {
      console.error("Erro ao realizar logout:", error);
      Alert.alert("Erro", "Falha ao realizar logout.");
    }
  };


    return (
      <View style={styles.container}>
        <TouchableOpacity onPress={handleLogout} style={styles.IconSair}>
          <Ionicons name="log-out-outline" size={35} color="black"/>
        </TouchableOpacity>
        <Text style={styles.titleTela}>Turmas</Text>
        <TouchableOpacity 
          style={styles.addButtonStyle} 
          onPress={() => setModalVisible(true)}>
          <Text style={{ color: 'white'}}>Adicionar Turma</Text>
        </TouchableOpacity>
        <FlatList
          data={turmasData}
          keyExtractor={item => item.id}
          renderItem={({ item }) => (
            <View style={styles.containerTurmas}>
              <Text style={styles.titleTurmas}>{item.name}</Text>
              <ButtonPers 
              title={<Ionicons name="add-outline" size={25} color="white"/>} 
              style={styles.buttonStyleInsert} 
              onPress={()=> {
              setSelectedClassName(item.name);
              setModalAlunoVisible(true);
            }}
              />
              <ButtonPers title="Acessar" style={styles.buttonStyle} onPress={()=> navigation.navigate('Detalhes',{turma:item})}></ButtonPers>
              <ButtonPers title={<Ionicons name="trash-outline" size={20} color="white" />} style={styles.buttonStyleDelete} onPress={() => handleDeleteTurma(item.id)}></ButtonPers>
            </View>
          )}
        />
      <Modal
        animationType="slide"
        transparent={true}
        visible={modalVisible}
        onRequestClose={() => setModalVisible(false)} 
      >
        <View style={styles.modalContainer}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Adicionar Nova Turma</Text>
            <TextInput
              style={styles.input}
              placeholder="Nome da nova turma"
              value={newTurmaName}
              onChangeText={setNewTurmaName}
            />
            <View style={styles.modalButtons}>
              <Button title="Cancelar" onPress={() => setModalVisible(false)} />
              <Button title="Adicionar" onPress={handleAddTurma} />
            </View>
          </View>
        </View>
      </Modal>
        <Modal
        animationType="slide"
        transparent={true}
        visible={modalAlunoVisible}
        onRequestClose={() => setModalAlunoVisible(false)}
      >
        <View style={styles.modalContainer}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Adicionar Aluno</Text>
            <TextInput
              style={styles.input}
              placeholder="Nome do Aluno"
              value={newStudentName}
              onChangeText={setNewStudentName}
            />
            <View style={styles.modalButtons}>
              <Button title="Cancelar" onPress={() => setModalAlunoVisible(false)} />
              <Button title="Adicionar" onPress={handleInsertStudent} />
            </View>
          </View>
        </View>
      </Modal>

      </View>
    );}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    margin: 20,
    top: 10,
  },
  containerTurmas: {
    backgroundColor: "grey",
    height: 120,
    margin: 10,
    justifyContent: "center",
    alignItems: "center",
    borderRadius: 10,
  },
  addButtonStyle: {
    backgroundColor: "#96CA5E",
    height: 40,
    width: "95%",
    borderRadius: 10,
    justifyContent: "center",
    alignItems: "center",
    marginBottom: 20,
    marginLeft: 10,
  },
  modalContent: {
    width: "80%",
    backgroundColor: "white",
    padding: 20,
    borderRadius: 10,
    alignItems: "center",
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: "bold",
    marginBottom: 20,
  },
  modalButtons: {
    flexDirection: "row",
    justifyContent: "space-between",
    width: "100%",
  },
  modalContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "rgba(0, 0, 0, 0.5)", 
  },
  titleTurmas: {
    color: "white",
    marginBottom: 60,
  },
  titleAlunos: {
    color: "white",
    position: "absolute",
    left: 30,
    top: 80,
  },
  buttonStyle: {
    backgroundColor: "#96CA5E",
    height: 30,
    width: 80,
    justifyContent: "center",
    alignItems: "center",
    borderRadius: 10,
    position: "absolute",
    right: 20,
    top: 75,
  },
  buttonStyleInsert: {
    backgroundColor: "#3941ff",
    height: 30,
    width: 80,
    justifyContent: "center",
    alignItems: "center",
    borderRadius: 10,
    position: "absolute",
    left: 30,
    top: 75,
  },
  buttonStyleDelete: {
    backgroundColor: "#de2c4b",
    height: 30,
    width: 80,
    justifyContent: "center",
    alignItems: "center",
    borderRadius: 10,
    position: "absolute",
    top: 75,
  },
  titleTela: {
    fontSize: 50,
    paddingLeft: 92,
    marginBottom: 33,
    marginTop: 30,
  },
  IconSair: {
    marginLeft: 335,
    height: 50,
    width: 60,
    top: 10,
  },
});
