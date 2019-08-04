<template>
  <div>
    <h1>Konfigürasyonlar</h1>

    <div class="add">
      <router-link :to="{name: 'Create'}" class="btn btn-primary">Ekle</router-link>
      <input type="text" v-model="search" placeholder="Ara.." />
    </div>
    <table class="table table-hover">
      <thead>
        <tr>
          <td>ID</td>
          <td>Kod</td>
          <td>Değer</td>
          <td>Tip</td>
          <td>Aktif</td>
          <td>Etki Alanı</td>
          <td colspan="2">İşlemler</td>
        </tr>
      </thead>

      <tbody>
        <tr v-for="item in filteredList" :key="item.id">
          <td>{{ item.id }}</td>
          <td>{{ item.name }}</td>
          <td>{{ item.value }}</td>
          <td>{{ item.type }}</td>
          <td>{{ item.isActive == true ? "Aktif" : "Pasif" }}</td>
          <td>{{ item.applicationName }}</td>
          <td>
            <router-link
              :to="{name: 'Edit', params: { id: item.id }}"
              class="btn btn-primary"
            >Düzenle</router-link>
          </td>
          <td>
            <button class="btn btn-danger" v-on:click="deleteItem(item.id)">Sil</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      items: [],
      search: ""
    };
  },

  created: function() {
    this.fetchItems();
  },

  methods: {
    fetchItems() {
      let uri = "https://localhost:5001/api/configuration";
      this.axios.get(uri).then(response => {
        this.items = response.data;
      });
    },
    deleteItem(id) {
      debugger;
      let uri = "https://localhost:5001/api/configuration/" + id;
      let i = this.items.map(item => item.id).indexOf(id); // find index of your object
      this.items.splice(i, 1); // remove it from array
      this.axios.delete(uri);
    }
  },
  computed: {
    filteredList() {
      return this.items.filter(item => {
        return item.name.toLowerCase().includes(this.search.toLowerCase());
      });
    }
  }
};
</script>

<style>
.add {
  float: right;
  margin: 10px 0;
}
.add input {
  float: left;
  min-height: 40px;
  margin-right: 10px;
}
</style>
