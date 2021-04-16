<template>
<div id='searchbar'>
  <!-- <input id='search-bar' @input="onSearch" class="form-control" type="text" placeholder="Search" aria-label="Search"/> -->
  <b-input-group class="mt-3" id='search-bar'>
    <b-form-input v-model="text" @input="onSearch" placeholder="Search"></b-form-input>
    <b-input-group-append>
      <b-button variant="info">&#x2715;</b-button>
    </b-input-group-append>
  </b-input-group>
</div>
</template>

<script>
export default {
  data() {
    return {
      text: ''
    }
  },
  methods: {
    // This method runs every time an input is given to the search bar
    // Emits the search text so other components can access it
    onSearch () {
      this.$emit('input', this.text)
    }
  }
}
</script>

<style scoped>
#search-bar {
  width: 20%;
  left: 20px;
}
</style>
